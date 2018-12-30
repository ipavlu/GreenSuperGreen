using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public class ConcurrentQueueWriterWorker : BenchmarkWorker
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueWriterWorker(
			ConcurrentQueue<object> queue,
			IBenchmarkConfiguration benchmarkConfiguration,
			string pair = null)
			: base(benchmarkConfiguration)
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