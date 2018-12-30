using System;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncBenchmark<TAsynLockUC> : Benchmark<TAsynLockUC> where TAsynLockUC : class, IAsyncLockUC, new()
	{
		protected override TAsynLockUC SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new TAsynLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount / 2;

		protected override BenchmarkWorker[] BenchInstanceGenerator(TAsynLockUC syncprimitive, IThreadGroupIndex threadGroupIndex) =>
			new BenchmarkWorker[]
			{
				new AsyncLockWorker(syncprimitive, this, $"{threadGroupIndex}:A"),
				new AsyncLockWorker(syncprimitive, this, $"{threadGroupIndex}:B")
			};
		protected NeighborAsyncBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}