using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncSpinLockUC : HeavyAsyncBenchmark<AsyncSpinLockUC>
	{
		public HeavyAsyncSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}