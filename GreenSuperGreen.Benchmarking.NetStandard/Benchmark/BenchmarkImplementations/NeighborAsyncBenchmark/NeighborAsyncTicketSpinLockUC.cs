using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncTicketSpinLockUC : NeighborAsyncBenchmark<AsyncTicketSpinLockUC>
	{
		public NeighborAsyncTicketSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}