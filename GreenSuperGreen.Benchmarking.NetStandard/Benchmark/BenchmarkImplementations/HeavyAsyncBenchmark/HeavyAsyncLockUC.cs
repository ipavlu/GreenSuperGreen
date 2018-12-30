using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncLockUC : HeavyAsyncBenchmark<AsyncLockUC>
	{
		public HeavyAsyncLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}
