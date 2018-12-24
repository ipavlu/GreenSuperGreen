using System;
using System.Threading;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="MutexLockUC"/> is based on .Net <see cref="Mutex"/>, the synchronization primitive behind c# lock keyword.
	/// <para/> Wrapper does not support recursive call and does not protect against recursive call even when the <see cref="Monitor"/> supports it!
	/// <para/> Enter and Exit MUST BE DONE on same thread! The lock is thread affine!
	/// <para/> awaiting inside entry block NOT SUPPORTED, causing synchronization error!
	/// </summary>
	[Obsolete("Potentially harmfull! You would be served better with LockUC! Awaiting inside entry block or attempt to exit entry block in different thread is not supported!")]
	internal class MutexLockUC : ILockUC
	{
		private Mutex Mutex { get; } = new Mutex();
		private EntryCompletionUC EntryCompletion { get; }
		
		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.ThreadAffine
		;

		public MutexLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit() => Mutex.ReleaseMutex();

		public EntryBlockUC Enter()
		{
			Mutex.WaitOne();
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public EntryBlockUC TryEnter()
		{
			return Mutex.WaitOne(0)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			return Mutex.WaitOne(milliseconds)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}
	}
}
