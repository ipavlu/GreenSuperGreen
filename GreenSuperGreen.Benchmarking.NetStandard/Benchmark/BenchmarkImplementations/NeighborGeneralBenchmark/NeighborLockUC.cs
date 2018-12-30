using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborLockUC : NeighborGeneralBenchmark<LockUC>
	{
		public NeighborLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}