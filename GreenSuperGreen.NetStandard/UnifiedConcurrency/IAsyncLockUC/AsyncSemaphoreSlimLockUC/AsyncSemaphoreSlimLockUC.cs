using System.Threading;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	public class AsyncSemaphoreSlimLockUC : IAsyncLockUC
	{
		private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
		private EntryBlockUC ExclusiveEntry { get; }

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public AsyncSemaphoreSlimLockUC()
		{
			ExclusiveEntry = new EntryBlockUC(EntryTypeUC.Exclusive, new EntryCompletionUC(Exit));
		}

		private void Exit() => Semaphore.Release();

		public AsyncEntryBlockUC Enter() => new AsyncEntryBlockUC(ExclusiveEntry, Semaphore.WaitAsync());

		public AsyncEntryBlockUC TryEnter() => new AsyncEntryBlockUC(ExclusiveEntry, Semaphore.WaitAsync(0));

		public AsyncEntryBlockUC TryEnter(int milliseconds) => new AsyncEntryBlockUC(ExclusiveEntry, Semaphore.WaitAsync(milliseconds));
	}
}