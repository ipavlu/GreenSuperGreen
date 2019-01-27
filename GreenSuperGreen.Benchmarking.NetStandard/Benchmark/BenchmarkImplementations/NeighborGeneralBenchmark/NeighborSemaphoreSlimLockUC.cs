using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborSemaphoreSlimLockUC : NeighborGeneralBenchmark<SemaphoreSlimLockUC>
	{
		public NeighborSemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}