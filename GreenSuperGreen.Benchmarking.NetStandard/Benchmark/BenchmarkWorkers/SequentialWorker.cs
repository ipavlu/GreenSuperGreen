using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class SequentialWorker : BenchmarkWorker
	{
		public SequentialWorker(IBenchmarkConfiguration benchmarkConfiguration, string resourceName, string pair) : base(benchmarkConfiguration, resourceName, pair)
		{
		}

		protected override async Task BenchmarkingTarget()
		{
			WastingTime();
			await Task.CompletedTask;
		}
	}
}