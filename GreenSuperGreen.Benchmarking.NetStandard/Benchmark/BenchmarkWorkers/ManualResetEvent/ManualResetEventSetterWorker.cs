using System.Threading;
using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public class ManualResetEventSetterWorker : BenchmarkWorker
	{
		private ManualResetEvent mre { get; }
		public ManualResetEventSetterWorker(ManualResetEvent syncPrimitive, IBenchmarkConfiguration benchmarkConfiguration, string pair = null)
			: base(benchmarkConfiguration)
		{ mre = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			WastingTime();
			mre.Set();
			await Task.CompletedTask;
		}
	}
}