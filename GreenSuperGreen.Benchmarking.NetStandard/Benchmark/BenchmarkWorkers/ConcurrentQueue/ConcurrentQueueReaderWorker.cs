using System.Threading.Tasks;
using System.Collections.Concurrent;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class ConcurrentQueueReaderWorker : BenchmarkWorker
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueReaderWorker(
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
			while (!Queue.TryDequeue(out _)) { }
			await Task.CompletedTask;
		}
	}
}