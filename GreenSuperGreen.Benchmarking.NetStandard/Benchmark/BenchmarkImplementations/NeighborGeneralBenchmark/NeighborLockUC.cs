using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborLockUC : NeighborGeneralBenchmark<LockUC>
	{
		public NeighborLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}