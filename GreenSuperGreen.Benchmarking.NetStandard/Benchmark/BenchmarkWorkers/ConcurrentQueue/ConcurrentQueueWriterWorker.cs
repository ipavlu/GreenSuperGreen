using System.Collections.Concurrent;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class ConcurrentQueueWriterWorker : BenchmarkWorker
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueWriterWorker(
			ConcurrentQueue<object> queue,
			IBenchmarkConfiguration benchmarkConfiguration,
			string resourceName,
			string pair)
			: base(benchmarkConfiguration, resourceName, pair)
		{
			Queue = queue;
		}

		protected override async Task BenchmarkingTarget()
		{
			Queue.Enqueue(null);
			await Task.CompletedTask;
		}
	}
}