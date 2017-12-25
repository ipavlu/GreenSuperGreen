using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable TypeParameterCanBeVariant
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Queues
{
	/// <summary>
	/// <para/> Concurrent non-blocking priority queue with optional priority based dequeue.
	/// <para/> Do not use <see cref="System.Enum.GetValues(System.Type)"/> to buildDescendingPriorities
	/// as C# specification does not ensure order of values as defined in your enum unless you explicitly
	/// assign value to each enum name and order by value!
	/// <para/> Flagged enums are not supported.
	/// </summary>
	/// <typeparam name="TPrioritySelectorEnum"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public interface IPriorityQueueUC<TPrioritySelectorEnum, TItem> where TPrioritySelectorEnum : struct
	{
		void Enqueue(TPrioritySelectorEnum prioritySelector, TItem item);
		void Enqueue(TPrioritySelectorEnum prioritySelector, IList<TItem> items);
		void Enqueue(TPrioritySelectorEnum prioritySelector, IEnumerable<TItem> items);
		bool TryDequeu(out TItem item, TPrioritySelectorEnum? prioritySelector = null);
		bool TryDequeu(out TItem item, out TPrioritySelectorEnum? priority, TPrioritySelectorEnum? prioritySelector = null);
		int Count(TPrioritySelectorEnum? prioritySelector = null);
		bool HasItems(TPrioritySelectorEnum? prioritySelector = null);
	}
}
