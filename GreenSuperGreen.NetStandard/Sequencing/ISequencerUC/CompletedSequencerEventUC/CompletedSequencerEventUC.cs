using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal class CompletedSequencerEventUC : ACompletionUC<CompletedSequencerEventUC>
	{
		public
		static
		ICompletionUC
		CompletedAwaiter { get; } = new CompletedSequencerEventUC()
		;

		private CompletedSequencerEventUC()
		{
			SetCompletion();
		}
	}
}
