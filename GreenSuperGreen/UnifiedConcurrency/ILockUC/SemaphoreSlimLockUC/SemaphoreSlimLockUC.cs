using System.Threading;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="SemaphoreSlimLockUC"/> is based on .Net <see cref="SemaphoreSlim"/>.
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered...
	/// </summary>
	public class SemaphoreSlimLockUC : ILockUC
	{
		private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

		private EntryCompletionUC EntryCompletion { get; }

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public SemaphoreSlimLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit() => Semaphore.Release();

		public EntryBlockUC Enter()
		{
			Semaphore.Wait();
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public EntryBlockUC TryEnter()
		{
			return Semaphore.Wait(0)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			return Semaphore.Wait(milliseconds)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}
	}
}
