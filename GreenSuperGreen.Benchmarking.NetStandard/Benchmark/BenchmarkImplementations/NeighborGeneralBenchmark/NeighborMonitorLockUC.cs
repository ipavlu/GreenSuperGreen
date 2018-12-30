using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
#pragma warning disable 618
	internal class NeighborMonitorLockUC : NeighborGeneralBenchmark<MonitorLockUC>
	{
		public NeighborMonitorLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
#pragma warning restore 618
}