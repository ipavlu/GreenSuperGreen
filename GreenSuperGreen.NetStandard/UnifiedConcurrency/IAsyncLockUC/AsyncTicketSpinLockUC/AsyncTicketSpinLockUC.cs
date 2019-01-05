using System;
using System.Threading;
using GreenSuperGreen.Exceptions;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="AsyncTicketSpinLockUC"/> is based on .Net <see cref="Interlocked"/> operations (atomic instructions).
	/// <para/> Does not support recursive call and does not protect against recursive call!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preferred.
	/// <para/> TryEnter can not be used! Not supported!
	/// </summary>
	public class AsyncTicketSpinLockUC : IAsyncLockUC
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

		public AsyncTicketSpinLockUC() { EntryCompletion = new EntryCompletionUC(Exit); }

		private void Exit() => Interlocked.Increment(ref _activeTicket);

		public AsyncEntryBlockUC Enter()
		{
			Thread.BeginCriticalRegion();
			int ticket = Interlocked.Increment(ref _tickets);
			while (Interlocked.Add(ref _activeTicket, 0) != ticket) { }
			Thread.EndCriticalRegion();
			return new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		[Obsolete("AsyncTicketSpinLockUC does not support TryEnter entry!!!", true)]
		public AsyncEntryBlockUC TryEnter() { throw new IntentionallyNotImplementedException($"{nameof(AsyncTicketSpinLockUC)} does not support TryEnter entry!!!"); }

		[Obsolete("AsyncTicketSpinLockUC does not support TryEnter entry!!!", true)]
		public AsyncEntryBlockUC TryEnter(int milliseconds) { throw new IntentionallyNotImplementedException($"{nameof(AsyncTicketSpinLockUC)} does not support TryEnter entry!!!"); }
	}
}
