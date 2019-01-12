using System;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborAsyncBenchmark<TAsyncLockUC> : Benchmark<TAsyncLockUC> where TAsyncLockUC : class, IAsyncLockUC, new()
	{
		protected override TAsyncLockUC SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new TAsyncLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount / 2;

		protected override BenchmarkWorker[] BenchInstanceGenerator(TAsyncLockUC syncPrimitive, IThreadGroupIndex threadGroupIndex) =>
			new BenchmarkWorker[]
			{
				new AsyncLockWorker(syncPrimitive, this, $"{threadGroupIndex}", $"{threadGroupIndex}:A"),
				new AsyncLockWorker(syncPrimitive, this, $"{threadGroupIndex}",$"{threadGroupIndex}:B")
			};
		protected NeighborAsyncBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}