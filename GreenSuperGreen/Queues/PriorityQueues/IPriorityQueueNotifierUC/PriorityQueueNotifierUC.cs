using System.Collections.Concurrent;
using System.Collections.Generic;
using GreenSuperGreen.Async;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable UnusedMember.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable StaticMemberInGenericType

namespace GreenSuperGreen.Queues
{
	/// <summary>
	/// <para/> Concurrent non-blocking priority queue with optional priority based dequeue.
	/// <para/> Do not use <see cref="System.Enum.GetValues(System.Type)"/> to build DescendingPriorities
	/// as C# specification does not ensure order of values as defined in your enum unless you explicitly
	/// assign value to each enum name and order by value!
	/// <para/> Flagged enums are not supported.
	/// </summary>
	/// <typeparam name="TPrioritySelectorEnum"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public
	class
	PriorityQueueNotifierUC<TPrioritySelectorEnum, TItem>
	:		PriorityQueueUC<TPrioritySelectorEnum, TItem>,
			IPriorityQueueNotifierUC<TPrioritySelectorEnum, TItem>,
			IPriorityQueueUC<TPrioritySelectorEnum, TItem>
	where	TPrioritySelectorEnum : struct
	{
		private ISimpleLockUC Lock { get; } = new SpinLockUC();

		private ConcurrentQueue<IEnqueuedCompletionUC> NotifyQueue { get; } 
		= new ConcurrentQueue<IEnqueuedCompletionUC>()
		;

		/// <summary>
		/// <para/> Concurrent non-blocking priority queue with optional priority based dequeue.
		/// <para/> Do not use <see cref="System.Enum.GetValues(System.Type)"/> to build <see cref="descendingPriorities"/>
		/// as C# specification does not ensure order of values as defined in your enum unless you explicitly
		/// assign value to each enum name and order by value!
		/// <para/> Flagged enums are not supported.
		/// </summary>
		public PriorityQueueNotifierUC(IEnumerable<TPrioritySelectorEnum> descendingPriorities)
			: base(descendingPriorities)
		{
		}

		/// <summary>
		/// If <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public override void Enqueue(TPrioritySelectorEnum prioritySelector, TItem item)
		{
			base.Enqueue(prioritySelector, item);

			using (Lock.Enter())
			{
				IEnqueuedCompletionUC enqueued;
				while (NotifyQueue.TryDequeue(out enqueued))
				{
					enqueued?.Enqueued();
				}
			}
		}

		/// <summary>
		/// With multiple readers this servers only as notification about Enquing data
		/// or already enqueued data, but other readers could be faster to get the data.
		/// Thus for some readers queues could be already empty.
		/// </summary>
		public ICompletionUC EnqueuedItemsAsync()
		{
			using (Lock.Enter())
			{
				if (HasItems()) return EnqueuedCompletionUC.AlreadyEnqueued;
				IEnqueuedCompletionUC enqueued;
				NotifyQueue.Enqueue(enqueued = new EnqueuedCompletionUC());
				return enqueued;
			}
		}
	}
}
