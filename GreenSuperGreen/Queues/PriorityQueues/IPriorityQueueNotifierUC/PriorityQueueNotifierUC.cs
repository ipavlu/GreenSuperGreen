using System.Collections.Generic;
using System.Linq;
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

		private Queue<AsyncEnqueuedCompletionUC> NotifyAnyPriority { get; } = new Queue<AsyncEnqueuedCompletionUC>();
		private Dictionary<TPrioritySelectorEnum, Queue<AsyncEnqueuedCompletionUC>> NotifyPriority { get; }

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
			NotifyPriority = DescendingPriorities.ToDictionary(p => p, p => new Queue<AsyncEnqueuedCompletionUC>());
		}

		/// <summary>
		/// If <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public override void Enqueue(TPrioritySelectorEnum prioritySelector, TItem item)
		{
			base.Enqueue(prioritySelector, item);

			using (Lock.Enter())
			{
				int iMax = NotifyAnyPriority.Count;
				for (int i = 0; i < iMax; i++) NotifyAnyPriority.Dequeue().Enqueued();

				Queue<AsyncEnqueuedCompletionUC> priorityQueue = NotifyPriority[prioritySelector];
				iMax = priorityQueue.Count;
				for (int i = 0; i < iMax; i++) priorityQueue.Dequeue().Enqueued();
			}
		}

		/// <summary>
		/// Use only with TryDequeue without overridden priority!
		/// Awaitable returns completed as long as there are any items for any priority!
		/// </summary>
		public AsyncEnqueuedCompletionUC EnqueuedItemsAsync()
		{
			using (Lock.Enter())
			{
				if (HasItems()) return AsyncEnqueuedCompletionUC.AlreadyAsyncEnqueued;
				AsyncEnqueuedCompletionUC asyncEnqueued;
				NotifyAnyPriority.Enqueue(asyncEnqueued = new AsyncEnqueuedCompletionUC());
				return asyncEnqueued;
			}
		}

		/// <summary>
		/// Use only with TryDequeue with same priority!
		/// </summary>
		public AsyncEnqueuedCompletionUC EnqueuedItemsAsync(TPrioritySelectorEnum prioritySelector)
		{
			using (Lock.Enter())
			{
				if (HasItems(prioritySelector)) return AsyncEnqueuedCompletionUC.AlreadyAsyncEnqueued;
				AsyncEnqueuedCompletionUC asyncEnqueued;
				NotifyPriority[prioritySelector].Enqueue(asyncEnqueued = new AsyncEnqueuedCompletionUC());
				return asyncEnqueued;
			}
		}
	}
}
