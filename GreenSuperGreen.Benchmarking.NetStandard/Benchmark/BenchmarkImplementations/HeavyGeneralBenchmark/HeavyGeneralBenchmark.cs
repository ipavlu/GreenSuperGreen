using System;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyGeneralBenchmark<TLockUC> : Benchmark<TLockUC> where TLockUC : class, ILockUC, new()
	{
		protected override TLockUC SharedSyncPrimitiveFactory { get; } = new TLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount;
		protected override BenchmarkWorker[] BenchInstanceGenerator(TLockUC syncPrimitive, IThreadGroupIndex threadGroupIndex)
			=> new BenchmarkWorker[] { new GeneralLockWorker(syncPrimitive, this, "global", $"global:{threadGroupIndex}") }
		;
		protected HeavyGeneralBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}