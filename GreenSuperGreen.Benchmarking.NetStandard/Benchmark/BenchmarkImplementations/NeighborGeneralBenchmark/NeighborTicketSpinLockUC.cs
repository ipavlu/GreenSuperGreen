using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborTicketSpinLockUC : NeighborGeneralBenchmark<TicketSpinLockUC>
	{
		public NeighborTicketSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}