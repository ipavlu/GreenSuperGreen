using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
#pragma warning disable 618
	internal class NeighborMutexLockUC : NeighborGeneralBenchmark<MutexLockUC>
	{
		public NeighborMutexLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
#pragma warning restore 618
}