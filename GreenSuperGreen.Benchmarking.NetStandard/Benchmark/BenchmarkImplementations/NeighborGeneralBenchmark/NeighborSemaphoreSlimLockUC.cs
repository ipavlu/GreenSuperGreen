using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	internal class NeighborSemaphoreSlimLockUC : NeighborGeneralBenchmark<SemaphoreSlimLockUC>
	{
		public NeighborSemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}