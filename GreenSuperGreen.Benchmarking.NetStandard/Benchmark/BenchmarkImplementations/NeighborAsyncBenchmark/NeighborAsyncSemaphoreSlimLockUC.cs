using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncSemaphoreSlimLockUC : NeighborAsyncBenchmark<AsyncSemaphoreSlimLockUC>
	{
		public NeighborAsyncSemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}