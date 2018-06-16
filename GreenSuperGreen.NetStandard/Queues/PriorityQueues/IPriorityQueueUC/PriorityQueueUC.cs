using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InvertIf
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_BuiltInTypes
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable TypeParameterCanBeVariant
// ReSharper disable UnusedMember.Global
// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Queues
{
	/// <summary>
	/// <para/> Concurrent non-blocking priority queue with optional priority based dequeue.
	/// <para/> Do not use <see cref="Enum.GetValues(Type)"/> to build <see cref="DescendingPriorities"/>
	/// as C# specification does not ensure order of values as defined in your enum unless you explicitly
	/// assign value to each enum name and order by value!
	/// <para/> Flagged enums are not supported.
	/// </summary>
	/// <typeparam name="TPrioritySelectorEnum"></typeparam>
	/// <typeparam name="TItem"></typeparam>
	public
	class
	PriorityQueueUC<TPrioritySelectorEnum, TItem>
	: IPriorityQueueUC<TPrioritySelectorEnum, TItem>
		where TPrioritySelectorEnum : struct
	{
		public static bool IsNotEnum => PriorityEnumCheckerUC<TPrioritySelectorEnum>.IsNotEnum;
		public static bool IsFlaggedEnum => PriorityEnumCheckerUC<TPrioritySelectorEnum>.IsFlaggedEnum;
		public static void TestEnum() => PriorityEnumCheckerUC<TPrioritySelectorEnum>.TestEnum($"{nameof(PriorityQueueUC<TPrioritySelectorEnum, TItem>)}");

		protected TPrioritySelectorEnum[] DescendingPriorities { get; }

		private
		Dictionary<TPrioritySelectorEnum, ConcurrentQueue<TItem>> PriorityQueues { get; }
		= new Dictionary<TPrioritySelectorEnum, ConcurrentQueue<TItem>>()
		;

		/// <summary>
		/// <para/> Concurrent non-blocking priority queue with optional priority based dequeue.
		/// <para/> Do not use <see cref="Enum.GetValues(Type)"/> to build <see cref="descendingPriorities"/>
		/// as C# specification does not ensure order of values as defined in your enum unless you explicitly
		/// assign value to each enum name and order by value!
		/// <para/> Flagged enums are not supported.
		/// </summary>
		public PriorityQueueUC(IEnumerable<TPrioritySelectorEnum> descendingPriorities)
		{
			TestEnum();

			DescendingPriorities = descendingPriorities.ToArray();
			HashSet<TPrioritySelectorEnum> uniquePriorities = new HashSet<TPrioritySelectorEnum>();

			string msg = string.Empty;

			foreach (TPrioritySelectorEnum priority in DescendingPriorities)
			{
				if (!uniquePriorities.Add(priority))
				{
					if (string.IsNullOrEmpty(msg))
					{
						msg += $"{nameof(PriorityQueueUC<TPrioritySelectorEnum, TItem>)}.Constructor";
						msg += " - descending list of priorities must be unique, some value used multiple times:\r\n";
					}
					msg += $"{nameof(TPrioritySelectorEnum)}.{priority}\r\n";
				}
				PriorityQueues.Add(priority, new ConcurrentQueue<TItem>());
			}

			if (!string.IsNullOrEmpty(msg)) throw new InvalidOperationException(msg);
		}

		/// <summary>
		/// If <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public virtual void Enqueue(TPrioritySelectorEnum prioritySelector, TItem item)
		{
			GetQueue(prioritySelector).Enqueue(item);
		}

		/// <summary>
		/// If <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public virtual void Enqueue(TPrioritySelectorEnum prioritySelector, IList<TItem> items)
		{
			if (items == null || items.Count <= 0) return;
			for (int i = 0; i < items.Count; ++i)
			{
				Enqueue(prioritySelector, items[i]);
			}
		}

		/// <summary>
		/// If <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public virtual void Enqueue(TPrioritySelectorEnum prioritySelector, IEnumerable<TItem> items)
		{
			foreach (TItem item in items)
			{
				Enqueue(prioritySelector, item);
			}
		}

		public virtual bool TryDequeu(out TItem item, TPrioritySelectorEnum? prioritySelector = null)
		{
			TPrioritySelectorEnum? priority;
			return TryDequeu(out item, out priority, prioritySelector);
		}

		/// <summary>
		/// <para/> if <see cref="prioritySelector"/> is null, then trying dequeue based on descending priorities,
		/// <para/> if <see cref="prioritySelector"/> is supported value, then overriding descending priorities and taking only from seleced priority,
		/// <para/> if <see cref="prioritySelector"/> is not supported value => Exception
		/// </summary>
		public virtual bool TryDequeu(out TItem item, out TPrioritySelectorEnum? priority, TPrioritySelectorEnum? prioritySelector = null)
		{
			item = default(TItem);
			priority = null;

			if (prioritySelector.HasValue)
			{
				if (GetQueue(prioritySelector.Value).TryDequeue(out item))
				{
					priority = prioritySelector;
					return true;
				}
				return false;
			}

			for (int i = 0; i < DescendingPriorities.Length;++i)
			{
				if (GetQueue(DescendingPriorities[i]).TryDequeue(out item))
				{
					priority = DescendingPriorities[i];
					return true;
				}
			}
			return false;
		}

		private string PriorityStatusToString(TPrioritySelectorEnum priority)
		{
			ConcurrentQueue<TItem> queue;
			PriorityQueues.TryGetValue(priority, out queue);
			int cnt = queue?.Count ?? -1;
			if (queue == null) return $"[{priority}:queue does not exist!]";
			if (cnt <= 0) return $"[{priority}:empty]";
			return $"[{priority}:{cnt}]";
		}

		public override string ToString()
		{
			return
			DescendingPriorities
			.Select(PriorityStatusToString)
			.Aggregate($"{nameof(PriorityQueueUC<TPrioritySelectorEnum, TItem>)}", (c, n) => c + "\r\n" + n)
			;
		}

		/// <summary>
		/// Unused priority or null queue in dictionary => Exception!
		/// </summary>
		/// <param name="prioritySelector"></param>
		/// <returns></returns>
		private ConcurrentQueue<TItem> GetQueue(TPrioritySelectorEnum prioritySelector)
		{
			ConcurrentQueue<TItem> queue;
			if (!PriorityQueues.TryGetValue(prioritySelector, out queue))
			{
				string msg = $"The value of {nameof(TPrioritySelectorEnum)}.{prioritySelector} is not supported!";
				Exception ex = new ArgumentException(msg);
				throw ex;
			}
			if (queue == null)
			{
				string msg = $"Priority queue for {nameof(TPrioritySelectorEnum)}.{prioritySelector} does not exist!";
				Exception ex = new ArgumentNullException(nameof(queue), msg);
				throw ex;
			}
			return queue;
		}

		/// <summary>
		/// <para/> when null, counts all existing priority queues,
		/// <para/> when priority is provided, then only count of given priority queue,
		/// <para/> when priority is provided and not supported, priority queue does not exist => Exception
		/// </summary>
		public int Count(TPrioritySelectorEnum? prioritySelector = null)
		{
			if (prioritySelector.HasValue)
			{
				return GetQueue(prioritySelector.Value).Count;
			}

			int cnt = 0;
			for (int i = 0; i < DescendingPriorities.Length; ++i)
			{
				cnt += GetQueue(DescendingPriorities[i]).Count;
			}
			return cnt;
		}

		/// <summary>
		/// <para/> Based on <see cref="ConcurrentQueue{T}.Count"/> without blocking Enqueue and TryDequeue operations!
		/// <para/> Under concurrent operations result is only informative!
		/// <para/> If <see cref="prioritySelector"/> is null, count of all priority queues > 0 is true.
		/// <para/> If <see cref="prioritySelector"/> is existing priority, count of the priority queue > 0 is true.
		/// <para/> If <see cref="prioritySelector"/> is unused enum value as priority => Exception!
		/// </summary>
		public bool HasItems(TPrioritySelectorEnum? prioritySelector = null)
		{
			if (prioritySelector.HasValue)
			{
				return (GetQueue(prioritySelector.Value).Count) > 0;
			}

			for (int i = 0; i < DescendingPriorities.Length; ++i)
			{
				if (GetQueue(DescendingPriorities[i]).Count > 0) return true;
			}
			return false;
		}
	}
}
