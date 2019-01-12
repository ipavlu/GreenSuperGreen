using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncSpinLockUC : NeighborAsyncBenchmark<AsyncSpinLockUC>
	{
		public NeighborAsyncSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}