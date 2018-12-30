using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GreenSuperGreen.Benchmarking
{
	public class ConcurrentQueueReaderWorker : BenchmarkWorker
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueReaderWorker(
			ConcurrentQueue<object> queue,
			IBenchmarkConfiguration benchmarkConfiguration,
			string pair = null)
			: base(benchmarkConfiguration)
		{
			Queue = queue;
		}

		protected override async Task BenchmarkingTarget()
		{
			object rslt;
			while (!Queue.TryDequeue(out rslt)) { }
			await Task.CompletedTask;
		}
	}
}