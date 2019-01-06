using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyLockUC : HeavyGeneralBenchmark<LockUC>
	{
		public HeavyLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}