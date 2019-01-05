using System;
using System.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="AsyncSpinLockUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
	/// Obviously awaiting does not happen here, await is returning back with completed result. Either locked or refused immediately.
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preferred...
	/// </summary>
	public class AsyncSpinLockUC : IAsyncLockUC
	{
		/// <summary> CAN NOT BE READONLY FIELD!!! CAN NOT BE PROPERTY!!! CAN NOT TRACK THREAD!!! </summary>
		private SpinLock _spinLock = new SpinLock(false);

		private EntryCompletionUC EntryCompletion { get; }

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public AsyncSpinLockUC() { EntryCompletion = new EntryCompletionUC(Exit); }

		/// <summary> used memory barrier, little less performing, but ensures fairness on heavy loaded boxes </summary>
		private void Exit() => _spinLock.Exit(true);

		public AsyncEntryBlockUC Enter()
		{
			bool gotLock = false;
			_spinLock.Enter(ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(SpinLockUC)}.{nameof(Enter)}: failed to acquire access");
			return new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public AsyncEntryBlockUC TryEnter()
		{
			bool gotLock = false;
			_spinLock.TryEnter(ref gotLock);
			return gotLock ? new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion) : AsyncEntryBlockUC.RefusedEntry;
		}

		public AsyncEntryBlockUC TryEnter(int milliseconds)
		{
			bool gotLock = false;
			_spinLock.TryEnter(milliseconds, ref gotLock);
			return gotLock ? new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion) : AsyncEntryBlockUC.RefusedEntry;
		}
	}
}
