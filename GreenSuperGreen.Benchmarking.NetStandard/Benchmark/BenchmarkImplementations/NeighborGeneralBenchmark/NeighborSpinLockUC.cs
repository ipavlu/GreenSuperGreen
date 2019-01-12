using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborSpinLockUC : NeighborGeneralBenchmark<SpinLockUC>
	{
		public NeighborSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}