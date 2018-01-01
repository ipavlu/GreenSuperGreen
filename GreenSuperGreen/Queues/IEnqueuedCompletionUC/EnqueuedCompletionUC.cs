using GreenSuperGreen.Async;

// ReSharper disable RedundantArgumentDefaultValue
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
		private EnqueuedCompletionUC(bool enqueued) : base(ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			if (enqueued) Enqueued();
		}
		public void Enqueued() => SetCompletion();
	}
}
