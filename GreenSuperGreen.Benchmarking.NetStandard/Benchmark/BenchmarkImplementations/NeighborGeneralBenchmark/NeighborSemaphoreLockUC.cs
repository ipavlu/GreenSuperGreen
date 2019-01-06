using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	internal class NeighborSemaphoreLockUC : NeighborGeneralBenchmark<SemaphoreLockUC>
	{
		public NeighborSemaphoreLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}