using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		private static ICompletionUC CompletedAwaiter { get; } = CompletedSequencerEventUC.CompletedAwaiter;
	}
}
