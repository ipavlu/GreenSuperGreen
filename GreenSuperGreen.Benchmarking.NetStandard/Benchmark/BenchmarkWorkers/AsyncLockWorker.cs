using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class AsyncLockWorker : BenchmarkWorker
	{
		private IAsyncLockUC Lock { get; }
		public AsyncLockWorker(	IAsyncLockUC syncPrimitive,
								IBenchmarkConfiguration benchmarkConfiguration,
								string resourceName,
								string pair)
		:	base(benchmarkConfiguration, resourceName, pair)
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