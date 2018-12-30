using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	internal class HeavyTicketSpinLockUC : HeavyGeneralBenchmark<TicketSpinLockUC>
	{
		public HeavyTicketSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}