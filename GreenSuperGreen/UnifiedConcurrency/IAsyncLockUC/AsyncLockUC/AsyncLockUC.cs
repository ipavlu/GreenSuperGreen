using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> </summary>
	public class AsyncLockUC : IAsyncLockUC
	{
		private struct AccessItem
		{
			private TaskCompletionSource<EntryBlockUC> StoredTCS { get; }
			private Task<TaskCompletionSource<EntryBlockUC>> StoredTaskTCS { get; }
			public TaskCompletionSource<EntryBlockUC> TCS => StoredTCS ?? StoredTaskTCS?.Result;

			public bool TrySetResult(EntryBlockUC result)
			{
				bool setRslt = TCS.TrySetResult(result);
				if (StoredTCS != null && setRslt) TimerProcessorUC.TimerProcessor.UnRegisterAsync(TCS);
				return setRslt;
			}

			public static AccessItem NewTCS() => new AccessItem(new TaskCompletionSource<EntryBlockUC>(TaskCreationOptions.RunContinuationsAsynchronously));

			public static AccessItem NewTimeLimitedTCS(int limit) => new AccessItem(limit);

			private AccessItem(TaskCompletionSource<EntryBlockUC> storedTCS)
			{
				StoredTCS = storedTCS;
				StoredTaskTCS = null;
			}

			private AccessItem(int limit)
			{
				StoredTCS = null;
				limit = limit < 2 ? 2 : limit;
				TimeSpan tsLimit = TimeSpan.FromMilliseconds(limit);
				StoredTaskTCS = TimerProcessorUC.TimerProcessor.RegisterResultAsync(tsLimit, EntryBlockUC.RefusedEntry);
			}
		}

		private Queue<AccessItem> Queue { get; } = new Queue<AccessItem>();
		private EntryBlockUC ExclusiveEntry { get; }
		private Status LockStatus { get; set; } = Status.Opened;

		/// <summary> CAN NOT BE READONLY FIELD!!! CAN NOT BE PROPERTY!!! CAN NOT TRACK THREAD!!! </summary>0
		private SpinLock _spinLock = new SpinLock(false);

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public AsyncLockUC()
		{
			ExclusiveEntry = new EntryBlockUC(EntryTypeUC.Exclusive, new EntryCompletionUC(Exit));
		}

		private void Exit()
		{
			while (true)
			{
				AccessItem access;
				bool gotLock = false;
				try
				{
					_spinLock.Enter(ref gotLock);
					if (Queue.Count == 0)
					{
						LockStatus = Status.Opened;
						return;
					}
					access = Queue.Dequeue();
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}
				if (access.TrySetResult(ExclusiveEntry)) return;
			}
		}

		private enum Status { Opened, Locked }

		public AsyncEntryBlockUC Enter()
		{
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(ExclusiveEntry);
				}
				AccessItem access;
				Queue.Enqueue(access = AccessItem.NewTCS());
				return new AsyncEntryBlockUC(null, access.TCS);
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
		}

		public AsyncEntryBlockUC TryEnter()
		{
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Locked) return AsyncEntryBlockUC.RefusedEntry;
				LockStatus = Status.Locked;
				return new AsyncEntryBlockUC(ExclusiveEntry);
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
		}

		public AsyncEntryBlockUC TryEnter(int milliseconds)
		{
			AccessItem access;
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(ExclusiveEntry);
				}
				Queue.Enqueue(access = AccessItem.NewTimeLimitedTCS(milliseconds));
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
			return new AsyncEntryBlockUC(null, access.TCS);
		}
	}
}
