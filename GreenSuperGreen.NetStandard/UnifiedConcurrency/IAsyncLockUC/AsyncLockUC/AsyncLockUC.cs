using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	public class AsyncLockUC : IAsyncLockUC
	{
		private struct AccessItem
		{
			private TaskCompletionSource<EntryBlockUC> StoredTCS { get; }
			private TaskCompletionSource<EntryBlockUC> StoredTimingProcessorTCS { get; }
			public TaskCompletionSource<EntryBlockUC> TCS => StoredTCS ?? StoredTimingProcessorTCS;

			public bool TrySetResult(EntryBlockUC result)
			{
				bool setResult = TCS.TrySetResult(result);
				if (StoredTimingProcessorTCS != null && setResult) TimerProcessorUC.TimerProcessor.UnRegisterAsync(TCS);
				return setResult;
			}

			public static AccessItem NewTCS() => new AccessItem(new TaskCompletionSource<EntryBlockUC>(TaskCreationOptions.RunContinuationsAsynchronously));

			public static AccessItem NewTimeLimitedTCS(int limit) => new AccessItem(limit);

			private AccessItem(TaskCompletionSource<EntryBlockUC> storedTCS)
			{
				StoredTCS = storedTCS;
				StoredTimingProcessorTCS = null;
			}

			private AccessItem(int limit)
			{
				StoredTCS = null;
				limit = limit < 2 ? 2 : limit;
				TimeSpan tsLimit = TimeSpan.FromMilliseconds(limit);
				StoredTimingProcessorTCS = TimerProcessorUC.TimerProcessor.RegisterResultAsync(tsLimit, EntryBlockUC.RefusedEntry).TCS;
			}
		}

		private ILockUC SpinLock { get; } = new SpinLockUC();
		private Queue<AccessItem> Queue { get; } = new Queue<AccessItem>();
		private EntryBlockUC ExclusiveEntry { get; }
		private Status LockStatus { get; set; } = Status.Opened;
		private enum Status { Opened, Locked }

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

		public AsyncEntryBlockUC Enter()
		{
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(ExclusiveEntry);
				}
				AccessItem access;
				Queue.Enqueue(access = AccessItem.NewTCS());
				return new AsyncEntryBlockUC(null, access.TCS);
			}
		}

		public AsyncEntryBlockUC TryEnter()
		{
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Locked) return AsyncEntryBlockUC.RefusedEntry;
				LockStatus = Status.Locked;
				return new AsyncEntryBlockUC(ExclusiveEntry);
			}
		}

		public AsyncEntryBlockUC TryEnter(int milliseconds)
		{
			AccessItem access;
			using (SpinLock.Enter())
			{
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(ExclusiveEntry);
				}
				Queue.Enqueue(access = AccessItem.NewTimeLimitedTCS(milliseconds));
			}
			return new AsyncEntryBlockUC(null, access.TCS);
		}
	}
}