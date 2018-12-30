using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public class SequentialWorker : BenchmarkWorker
	{
		public SequentialWorker(IBenchmarkConfiguration benchmarkConfiguration, string pair = null) 
		:	base(benchmarkConfiguration)
		{
		}

		protected override async Task BenchmarkingTarget()
		{
			WastingTime();
			await Task.CompletedTask;
		}
	}
}