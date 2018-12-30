using System.Threading;
using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public class ManualResetEventReSetterWorker : BenchmarkWorker
	{
		private ManualResetEvent mre { get; }
		public ManualResetEventReSetterWorker(ManualResetEvent syncPrimitive, IBenchmarkConfiguration benchmarkConfiguration, string pair = null)
			: base(benchmarkConfiguration)
		{ mre = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			mre.Reset();
			await Task.CompletedTask;
		}
	}
}