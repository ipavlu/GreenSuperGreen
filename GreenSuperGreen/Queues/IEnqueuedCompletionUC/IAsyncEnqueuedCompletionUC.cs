using System.Runtime.CompilerServices;
using GreenSuperGreen.Async;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Queues
{
	public interface IEnqueuedCompletionUC : ISimpleCompletionUC, INotifyCompletion, ICriticalNotifyCompletion
	{
		void Enqueued();
	}
}
