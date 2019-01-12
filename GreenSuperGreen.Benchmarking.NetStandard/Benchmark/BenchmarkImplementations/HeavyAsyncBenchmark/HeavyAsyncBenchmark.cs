using System;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavyAsyncBenchmark<TAsyncLockUC> : Benchmark<TAsyncLockUC> where TAsyncLockUC: class, IAsyncLockUC, new()
	{
		protected override TAsyncLockUC SharedSyncPrimitiveFactory { get; } = new TAsyncLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount;
		protected override BenchmarkWorker[] BenchInstanceGenerator(TAsyncLockUC syncPrimitive, IThreadGroupIndex threadGroupIndex)
		=> new BenchmarkWorker[] { new AsyncLockWorker(syncPrimitive, this, "global", $"global:{threadGroupIndex}") }
		;
		protected HeavyAsyncBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}