using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Collections.Concurrent
{
	public class ConcurrentDistinctInOrderCollection<T> : IConcurrentDistinctInOrderCollection<T>
	{
		private ILockUC Lock { get; } = new SpinLockUC();
		private ISet<T> DistinctSet { get; } = new HashSet<T>();
		private ImmutableArray<T> DistinctInOrderArrayStorage { get; set; } = ImmutableArray<T>.Empty;

		public ConcurrentDistinctInOrderCollection()
		{
		}

		public ConcurrentDistinctInOrderCollection(IEnumerable<T> items)
		{
			items
			.Where(DistinctSet.Add)
			.ToImmutableArray()
			.Assign(arr => DistinctInOrderArrayStorage = arr)
			;
		}

		public ImmutableArray<T> DistinctInOrderArray
		{
			get
			{
				using (Lock.Enter()) return DistinctInOrderArrayStorage;
			}
		}

		public ImmutableArray<T> Add(T item)
		{
			using (Lock.Enter())
			{
				return DistinctInOrderArrayStorage = DistinctSet.Add(item)
				? DistinctInOrderArrayStorage.Add(item)
				: DistinctInOrderArrayStorage
				;
			}
		}

		public ImmutableArray<T> AddRange(IEnumerable<T> items)
		{
			ImmutableArray<T> arr = (items ?? Enumerable.Empty<T>()).ToImmutableArray();

			using (Lock.Enter())
			{
				if (arr.IsEmpty) return DistinctInOrderArrayStorage;

				arr
				.Where(DistinctSet.Add)
				.ToImmutableArray()
				.AssignOut(out arr)
				;

				return arr.IsEmpty
				? DistinctInOrderArrayStorage
				: DistinctInOrderArrayStorage = DistinctInOrderArrayStorage.AddRange(arr)
				;
			}
		}

		public ImmutableArray<T> Remove(T item)
		{
			using (Lock.Enter())
			{
				return DistinctSet.Remove(item)
				? DistinctInOrderArrayStorage = DistinctInOrderArrayStorage.Remove(item)
				: DistinctInOrderArrayStorage
				;
			}
		}

		public bool IsEmpty
		{
			get
			{
				using (Lock.Enter())
				{
					return DistinctInOrderArrayStorage.IsEmpty;
				}
			}
		}

		public ImmutableArray<T> Clear()
		{
			using (Lock.Enter())
			{
				DistinctSet.Clear();
				return DistinctInOrderArrayStorage = ImmutableArray<T>.Empty;
			}
		}
	}
}