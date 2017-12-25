using GreenSuperGreen.Async;

// ReSharper disable UnusedMember.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.Queues
{
	public interface IPriorityQueueNotifierUC<TPrioritySelectorEnum, TItem>
	: IPriorityQueueUC<TPrioritySelectorEnum, TItem>
	where TPrioritySelectorEnum : struct
	{
		ICompletionUC EnqueuedItemsAsync();
	}
}
