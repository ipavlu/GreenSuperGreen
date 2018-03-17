using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="SpinLockUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered...
	/// </summary>
	public class LockUC : ISimpleLockUC
	{
		private struct AccessItem
		{
			private TaskCompletionSource<EntryBlockUC> StoredTCS { get; }
			private Task<TaskCompletionSource<EntryBlockUC>> StoredTaskTCS { get; }
			private TaskCompletionSource<EntryBlockUC> TCS => StoredTCS ?? StoredTaskTCS?.Result;

			public EntryBlockUC WaitForResult() => TCS.Task.Result;

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

		private ISimpleLockUC SpinLock { get; } = new SpinLockUC();
		private Queue<AccessItem> Queue { get; } = new Queue<AccessItem>();
		private EntryBlockUC ExclusiveEntry { get; }
		private Status LockStatus { get; set; } = Status.Opened;

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public LockUC()
		{
			ExclusiveEntry = new EntryBlockUC(EntryTypeUC.Exclusive, new EntryCompletionUC(Exit));
		}

		private void Exit()
		{
			while (true)
			{
				AccessItem access;
				using (SpinLock.Enter())
				{
					if (Queue.Count == 0)
					{
						LockStatus = Status.Opened;
						return;
					}
					access = Queue.Dequeue();
				}
				if (access.TrySetResult(ExclusiveEntry)) return;
			}
		}

		private enum Status { Opened, Locked }

		public EntryBlockUC Enter()
		{
			AccessItem access;
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return ExclusiveEntry;
				}
				Queue.Enqueue(access = AccessItem.NewTCS());
			}
			return access.WaitForResult();//waiting synchronously for completion
		}

		public EntryBlockUC TryEnter()
		{
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Locked) return EntryBlockUC.RefusedEntry;
				LockStatus = Status.Locked;
				return ExclusiveEntry;
			}
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			AccessItem access;
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return ExclusiveEntry;
				}
				Queue.Enqueue(access = AccessItem.NewTimeLimitedTCS(milliseconds));
			}

			return access.WaitForResult();//waiting synchronously for completion
		}
	}
}