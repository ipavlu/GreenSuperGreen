using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	internal class HeavySemaphoreLockUC : HeavyGeneralBenchmark<SemaphoreLockUC>
	{
		public HeavySemaphoreLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}