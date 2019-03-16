using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public abstract class ATestingJob : ITestingJob, IDisposable
	{
		private ConcurrentQueue<int> Source { get; } = new ConcurrentQueue<int>();
		private ConcurrentQueue<int> Destination { get; } = new ConcurrentQueue<int>();
		public int Count { get; }

		protected ATestingJob(int count)
		{
			Count = count;
			for (int i = 0; i < count; i++) Source.Enqueue(i);
		}

		public async Task Execute(int taskCount)
		{
			Enumerable
			.Range(0, taskCount)
			.Select(i => Task.Run((Action)TaskExecute))
			.ToArray()
			.AssignOut(out Task[] tasks)
			;

			await Task.WhenAll(tasks);
		}

		private void TaskExecute()
		{
			while (ExclusiveAccess())
			{
			}
		}

		protected abstract bool ExclusiveAccess();

		protected bool ProcessExclusively()
		{
			if (Source.TryDequeue(out int data))
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
				if (!Destination.TryDequeue(out int data)) throw new Exception("Destination is missing some data");
				if (data != i) throw new Exception("Locking process has been compromised, non exclusive access!!!");
			}
		}
	}
}
