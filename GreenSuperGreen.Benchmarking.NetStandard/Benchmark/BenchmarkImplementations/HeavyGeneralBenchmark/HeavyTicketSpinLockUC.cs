using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyTicketSpinLockUC : HeavyGeneralBenchmark<TicketSpinLockUC>
	{
		public HeavyTicketSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}