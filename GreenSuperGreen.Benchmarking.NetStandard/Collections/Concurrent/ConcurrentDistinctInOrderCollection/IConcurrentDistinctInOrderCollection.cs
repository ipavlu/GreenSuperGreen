using System.Collections.Generic;
using System.Collections.Immutable;

namespace GreenSuperGreen.Collections.Concurrent
{
	public interface IConcurrentDistinctInOrderCollection<T>
	{
		ImmutableArray<T> DistinctInOrderArray { get; }
		bool IsEmpty { get; }
		ImmutableArray<T> Add(T item);
		ImmutableArray<T> AddRange(IEnumerable<T> items);
		ImmutableArray<T> Remove(T item);
		ImmutableArray<T> Clear();
	}
}