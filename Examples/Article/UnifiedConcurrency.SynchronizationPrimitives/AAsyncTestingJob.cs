using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public abstract class AAsyncTestingJob : ITestingJob, IDisposable
	{
		private ConcurrentQueue<int> Source { get; } = new ConcurrentQueue<int>();
		private ConcurrentQueue<int> Destination { get; } = new ConcurrentQueue<int>();
		public int Count { get; }

		protected AAsyncTestingJob(int count)
		{
			Count = count;
			for (int i = 0; i < count; i++) Source.Enqueue(i);
		}

		public async Task Execute(int taskCount)
		{
			Task[] tasks =
			Enumerable
			.Range(0, taskCount)
			.Select(i => Task.Run(TaskExecute))
			.ToArray()
			;

			await Task.WhenAll(tasks);
		}

		private async Task TaskExecute()
		{
			while (await ExclusiveAccess())
			{
			}
		}

		protected abstract Task<bool> ExclusiveAccess();

		protected bool ProcessExclusively()
		{
			int data;
			if (Source.TryDequeue(out data))
			{
				Destination.Enqueue(data);
				return true;
			}
			return Source.Count > 0;
		}

		public void Dispose()
		{
			if (Source.Count != 0) throw new Exception("Source still has some data!");
			if (Destination.Count != Count) throw new Exception("Destination count is incorrect!");
			for (int i = 0; i < Count; i++)
			{
				int data;
				if (!Destination.TryDequeue(out data)) throw new Exception("Destination is missing some data");
				if (data != i) throw new Exception("Locking process has been compromised, non exclusive access!!!");
			}
		}
	}
}
