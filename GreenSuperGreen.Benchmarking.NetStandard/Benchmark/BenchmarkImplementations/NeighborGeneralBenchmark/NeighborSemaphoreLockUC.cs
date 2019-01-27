using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborSemaphoreLockUC : NeighborGeneralBenchmark<SemaphoreLockUC>
	{
		public NeighborSemaphoreLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}