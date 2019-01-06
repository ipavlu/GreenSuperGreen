using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncSemaphoreSlimLockUC : HeavyAsyncBenchmark<AsyncSemaphoreSlimLockUC>
	{
		public HeavyAsyncSemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}