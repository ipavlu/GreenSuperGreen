
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using GreenSuperGreen.Sequencing;

namespace GreenSuperGreen.Timing
{
	public enum OrderedExpiryItemsSequencer
	{
		TryExpireBegin,
		TryExpireEnd
	}

	public interface IOrderedExpiryItems
	{
		int Count { get; }
		bool TryAdd(TimerProcessorItem item);
		bool TryRemove(TimerProcessorItem item);
		bool TryExpire(DateTime now);
		bool Any();
		void CancelAllAndClear();
	}

	public class OrderedExpiryItems : IOrderedExpiryItems
	{
		/// <summary> Key access wont be possible, only indexed/value-referenced removal of items from collection. </summary>
		private class DuplicityKeysComparer<TKey> : IComparer<TKey> where TKey : IComparable
		{
			public int Compare(TKey x, TKey y)
			{
				if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
				if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
				int result = x.CompareTo(y);
				return result == 0 ? 1 : result;
			}
		}

		private ISequencerUC NullSafeSequencer { get; }

		public OrderedExpiryItems(ISequencerUC nullSafeSequencer = null) { NullSafeSequencer = nullSafeSequencer; }

		private static DuplicityKeysComparer<DateTime> DuplicityComparer { get; } = new DuplicityKeysComparer<DateTime>();

		public int Count => SortedList.Count;

		//access to items by key not possible, due to duplicity comparer!
		private SortedList<DateTime, TimerProcessorItem> SortedList { get; } = new SortedList<DateTime, TimerProcessorItem>(DuplicityComparer);
		private Stack<int> Remove { get; } = new Stack<int>();

		public bool TryAdd(TimerProcessorItem item)
		{
			if (item.Expiry == null) return false;
			SortedList.Add(item.Expiry.Value, item);
			return true;
		}

		public bool TryRemove(TimerProcessorItem item)
		{
			int i = SortedList.IndexOfValue(item);
			if (i == -1) return false;
			SortedList.RemoveAt(i);
			return true;
		}

		public bool TryExpire(DateTime now)
		{
			for (int i = 0; i < SortedList.Count; ++i)
			{
				TimerProcessorItem item = SortedList.Values[i];
				if (now < item.Expiry) break;
				Remove.Push(i);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, OrderedExpiryItemsSequencer.TryExpireBegin, item);
				item.TryExpire();
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, OrderedExpiryItemsSequencer.TryExpireEnd, item);
			}

			bool somethingExpired = Remove.Count > 0;
			while (Remove.Count > 0) SortedList.RemoveAt(Remove.Pop());
			return somethingExpired;
		}

		public void CancelAllAndClear()
		{
			for (int i = 0; i < SortedList.Count; ++i)
			{
				TimerProcessorItem item = SortedList.Values[i];
				item.TryCancel();
			}
			SortedList.Clear();
		}

		public bool Any() => SortedList.Count > 0;
	}
}