using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class GeneralLockWorker : BenchmarkWorker
	{
		private ILockUC Lock { get; }
		public GeneralLockWorker(ILockUC syncPrimitive, IBenchmarkConfiguration benchmarkConfiguration, string pair = null)
			: base(benchmarkConfiguration)
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