using GreenSuperGreen.Async;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Queues
{
	public interface IEnqueuedCompletionUC : ICompletionUC
	{
		void Enqueued();
	}
}
