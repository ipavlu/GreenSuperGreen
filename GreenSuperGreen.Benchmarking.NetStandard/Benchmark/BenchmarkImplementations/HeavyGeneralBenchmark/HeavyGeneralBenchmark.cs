using System;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyGeneralBenchmark<TLockUC> : Benchmark<TLockUC> where TLockUC : class, ILockUC, new()
	{
		protected override TLockUC SharedSyncPrimitiveFactory { get; } = new TLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount;
		protected override BenchmarkWorker[] BenchInstanceGenerator(TLockUC syncprimitive, IThreadGroupIndex threadGroupIndex)
			=> new BenchmarkWorker[] { new GeneralLockWorker(syncprimitive, this, string.Empty) }
		;
		protected HeavyGeneralBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}