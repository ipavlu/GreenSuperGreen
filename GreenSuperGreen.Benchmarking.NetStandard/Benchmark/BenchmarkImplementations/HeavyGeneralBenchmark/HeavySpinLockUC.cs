using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavySpinLockUC : HeavyGeneralBenchmark<SpinLockUC>
	{
		public HeavySpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}