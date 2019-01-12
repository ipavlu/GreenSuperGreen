using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncTicketSpinLockUC : HeavyAsyncBenchmark<AsyncTicketSpinLockUC>
	{
		public HeavyAsyncTicketSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}