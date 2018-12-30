using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class AsyncLockWorker : BenchmarkWorker
	{
		private IAsyncLockUC Lock { get; }
		public AsyncLockWorker(IAsyncLockUC syncPrimitive, IBenchmarkConfiguration benchmarkConfiguration, string pair = null)
		:	base(benchmarkConfiguration)
		{ Lock = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			using (await Lock.Enter())
			{
				WastingTime();
			}
		}
	}
}