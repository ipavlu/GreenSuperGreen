using GreenSuperGreen.Async;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace GreenSuperGreen.Queues
{
	public class EnqueuedCompletionUC
	:	ACompletionUC<EnqueuedCompletionUC>,
		IEnqueuedCompletionUC,
		ICompletionUC
	{
		public static IEnqueuedCompletionUC AlreadyEnqueued { get; } = new EnqueuedCompletionUC(true);
		public EnqueuedCompletionUC() : this(false) { }
		public EnqueuedCompletionUC(bool enqueued)
		{
			if (enqueued) Enqueued();
		}
		public void Enqueued() => SetCompletion();
	}
}
