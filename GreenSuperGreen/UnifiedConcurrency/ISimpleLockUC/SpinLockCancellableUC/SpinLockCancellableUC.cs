//using System.Runtime.CompilerServices;
//using System.Threading;
//using System.Threading.Tasks;

//// ReSharper disable CheckNamespace
//// ReSharper disable InconsistentNaming
//// ReSharper disable SuggestVarOrType_BuiltInTypes

//namespace GreenSuperGreen.UnifiedConcurrency
//{
//	/// <summary>
//	/// <para/> <see cref="SpinLockCancellableUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
//	/// <para/> Does not support reentrancy and does not protect against reentrancy!
//	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered...
//	/// </summary>
//	public class SpinLockCancellableUC : ISimpleLockUC
//	{
//		/// <summary> CAN NOT BE READONLY FIELD!!! CAN NOT BE PROPERTY!!! CAN NOT TRACK THREAD!!! </summary>
//		private SpinLock _spinLock = new SpinLock(false);

//		private CancellationToken CancellationToken { get; }

//		private EntryCompletionUC EntryCompletion { get; }

//		public SyncPrimitiveCapabilityUC Capability { get; } = 0
//		| SyncPrimitiveCapabilityUC.Enter
//		| SyncPrimitiveCapabilityUC.TryEnter
//		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
//		| SyncPrimitiveCapabilityUC.Cancellable
//		| SyncPrimitiveCapabilityUC.NonReentrant
//		| SyncPrimitiveCapabilityUC.NonThreadAffine
//		;

//		public SpinLockCancellableUC(CancellationToken cancellationToken)
//		{
//			EntryCompletion = new EntryCompletionUC(Exit);
//			CancellationToken = cancellationToken;
//			CheckCancellationRequested("Cancelled during construction");
//		}

//		private void CheckCancellationRequested(string note = null, [CallerMemberName] string caller = "")
//		{
//			if (!CancellationToken.IsCancellationRequested) return;
//			string msg = $"{nameof(ISimpleLockUC)}:{nameof(SpinLockCancellableUC)}:{caller}: {note ?? string.Empty}";
//			throw new TaskCanceledException(msg);
//		}


//		private void Exit()
//		{
//			_spinLock.Exit(true);
//			//used memory barrier, little less performant,
//			//but ensures fairness on heavy loaded boxes
//			CheckCancellationRequested();
//		}

//		public EntryBlockUC Enter() => TryEnter(-1);

//		/// <summary>
//		/// Inifinite waiting if necessary, but here is simulated by 1ms steps,
//		/// after each step without access cancellation token is checked for cancellationrequest.
//		/// </summary>
//		public EntryBlockUC TryEnter()
//		{
//			CheckCancellationRequested();
//			bool gotLock = false;
//			_spinLock.TryEnter(ref gotLock);
//			return gotLock ? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion) : EntryBlockUC.RefusedEntry;
//		}

//		/// <summary>
//		/// Inifinite waiting if -1, but here is simulated by 1ms steps,
//		/// after each step without access cancellation token is checked for cancellationrequest.
//		/// Otherwise stepping and decrementing waiting time, aslo cancellation token checking.
//		/// </summary>
//		public EntryBlockUC TryEnter(int milliseconds)
//		{
//			CheckCancellationRequested();

//			if (milliseconds > 0)
//			{
//				bool gotLock = false;
//				while (milliseconds-- > 0)
//				{
//					_spinLock.TryEnter(milliseconds, ref gotLock);
//					if (gotLock) return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
//					CheckCancellationRequested();
//				}
//				return EntryBlockUC.RefusedEntry;
//			}

//			if (milliseconds == 0)
//			{
//				bool gotLock = false;
//				_spinLock.TryEnter(0, ref gotLock);
//				if (gotLock) return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
//				CheckCancellationRequested();
//				return EntryBlockUC.RefusedEntry;
//			}

//			if (milliseconds < 0)
//			{
//				bool gotLock = false;
//				while (true)
//				{
//					_spinLock.TryEnter(1, ref gotLock);
//					if (gotLock) return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
//					CheckCancellationRequested();
//				}
//			}

//			CheckCancellationRequested();
//			return EntryBlockUC.RefusedEntry;
//		}
//	}
//}
