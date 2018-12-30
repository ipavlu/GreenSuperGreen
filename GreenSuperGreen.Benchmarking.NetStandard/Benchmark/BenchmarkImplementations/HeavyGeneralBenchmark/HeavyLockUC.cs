using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyLockUC : HeavyGeneralBenchmark<LockUC>
	{
		public HeavyLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}