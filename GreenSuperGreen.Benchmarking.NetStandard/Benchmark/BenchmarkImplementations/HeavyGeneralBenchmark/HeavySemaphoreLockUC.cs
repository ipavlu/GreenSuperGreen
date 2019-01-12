using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	internal class HeavySemaphoreLockUC : HeavyGeneralBenchmark<SemaphoreLockUC>
	{
		public HeavySemaphoreLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}