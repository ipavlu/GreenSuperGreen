using System;
using System.Threading;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="TicketSpinLockUC"/> is based on .Net <see cref="Interlocked"/> operations (atomic instructions).
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered.
	/// <para/> TryEnter can not be used! Not supported!
	/// </summary>
	public class TicketSpinLockUC : ILockUC
	{
		private int _tickets = 0;
		private int _activeTicket = 1;

		private EntryCompletionUC EntryCompletion { get; }

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public TicketSpinLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit() => Interlocked.Increment(ref _activeTicket);

		public EntryBlockUC Enter()
		{
			Thread.BeginCriticalRegion();
			int ticket = Interlocked.Increment(ref _tickets);
			while (Interlocked.Add(ref _activeTicket, 0) != ticket) { }
			Thread.EndCriticalRegion();
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		[Obsolete("TicketSpinLockUC does not suport TryEnter entry!!!", true)]
		public EntryBlockUC TryEnter() { throw new NotImplementedException(); }

		[Obsolete("TicketSpinLockUC does not suport TryEnter entry!!!", true)]
		public EntryBlockUC TryEnter(int milliseconds) { throw new NotImplementedException(); }
	}
}
