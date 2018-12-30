using System;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborSpinLockUC : NeighborGeneralBenchmark<SpinLockUC>
	{
		public NeighborSpinLockUC(IBenchmarkConfiguration test) : base(test) { }
	}
}