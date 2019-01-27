using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavySemaphoreLockUC : HeavyGeneralBenchmark<SemaphoreLockUC>
	{
		public HeavySemaphoreLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}