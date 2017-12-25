using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace FFastFill.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="PureLockUC"/> is AutoResetEvent and <see cref="SpinLockUC"/> based lock.
	/// <para/> Does not support reentrancy and does not protect against reentrancy!
	/// <para/> Enter and Exit can be done on different threads.
	/// </summary>
	public class PureLockUC : ISimpleLockUC
	{
		private static ConcurrentDictionary<Thread, PureLockUC> WaitingThreads { get; }
		= new ConcurrentDictionary<Thread, PureLockUC>()
		;

		private SpinLockUC SpinLockUC { get; } = new SpinLockUC();
		private Queue<AutoResetEvent> LockQueue { get; } = new Queue<AutoResetEvent>();
		private EntryCompletionUC EntryCompletion { get; }

		private bool Locked { get; set; }

		public PureLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private PureLockUC UpdateFunc(Thread thread, PureLockUC previousPureLockUC) => this;

		private void Exit()
		{
			using (SpinLockUC.Enter())
			{

			}
		}

		public EntryBlockUC Enter()
		{
			using (SpinLockUC.Enter())
			{
				if (!Locked)
				{
					Locked = true;
					return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
				}

			}

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
