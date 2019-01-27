using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavySemaphoreSlimLockUC : HeavyGeneralBenchmark<SemaphoreSlimLockUC>
	{
		public HeavySemaphoreSlimLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}