using System;
using System.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="SpinLockUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preferred...
	/// </summary>
	public class SpinLockUC : ILockUC
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

		public SpinLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit()
		{
			_spinLock.Exit(true);
			//used memory barrier, little less performing, but ensures fairness on heavy loaded boxes
		}

		public EntryBlockUC Enter()
		{
			bool gotLock = false;
			_spinLock.Enter(ref gotLock);
			if (!gotLock) throw new InvalidOperationException($"{nameof(SpinLockUC)}.{nameof(Enter)}: failed to acquire access");
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public EntryBlockUC TryEnter()
		{
			bool gotLock = false;
			_spinLock.TryEnter(ref gotLock);
			return gotLock ? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion) : EntryBlockUC.RefusedEntry;
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			bool gotLock = false;
			_spinLock.TryEnter(milliseconds, ref gotLock);
			return gotLock ? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion) : EntryBlockUC.RefusedEntry;
		}
	}
}
