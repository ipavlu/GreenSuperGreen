using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	internal class HeavySemaphoreSlimLockUC : HeavyGeneralBenchmark<SemaphoreSlimLockUC>
	{
		public HeavySemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}