using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class GeneralLockWorker : BenchmarkWorker
	{
		private ILockUC Lock { get; }
		public GeneralLockWorker(	ILockUC syncPrimitive,
									IBenchmarkConfiguration benchmarkConfiguration,
									string resourceName,
									string pair)
			: base(benchmarkConfiguration, resourceName, pair)
		{ Lock = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			using (Lock.Enter())
			{
				WastingTime();
			}
			await Task.CompletedTask;
		}
	}
}