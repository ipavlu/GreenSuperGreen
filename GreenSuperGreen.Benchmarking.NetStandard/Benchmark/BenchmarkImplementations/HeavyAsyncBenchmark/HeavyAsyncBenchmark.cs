using System;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncBenchmark<TAsynLockUC> : Benchmark<TAsynLockUC> where TAsynLockUC: class, IAsyncLockUC, new()
	{
		protected override TAsynLockUC SharedSyncPrimitiveFactory { get; } = new TAsynLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount;
		protected override BenchmarkWorker[] BenchInstanceGenerator(TAsynLockUC syncprimitive, IThreadGroupIndex threadGroupIndex)
		=> new BenchmarkWorker[] { new AsyncLockWorker(syncprimitive, this, string.Empty) }
		;
		protected HeavyAsyncBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}