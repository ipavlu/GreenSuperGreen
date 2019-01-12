using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
#pragma warning disable CS0618 // Type or member is obsolete
	internal class HeavyMutexLockUC : HeavyGeneralBenchmark<MutexLockUC>
	{
		public HeavyMutexLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
#pragma warning restore CS0618 // Type or member is obsolete
}