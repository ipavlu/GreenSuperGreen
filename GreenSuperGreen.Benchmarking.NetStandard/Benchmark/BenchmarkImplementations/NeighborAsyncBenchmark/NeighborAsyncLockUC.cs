using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncLockUC : NeighborAsyncBenchmark<AsyncLockUC>
	{
		public NeighborAsyncLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}