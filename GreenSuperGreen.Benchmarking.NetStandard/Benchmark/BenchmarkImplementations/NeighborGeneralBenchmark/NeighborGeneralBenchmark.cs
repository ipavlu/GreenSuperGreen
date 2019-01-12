using System;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class NeighborGeneralBenchmark<TLockUC> : Benchmark<TLockUC> where TLockUC : class, ILockUC, new()
	{
		protected override TLockUC SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new TLockUC();
		protected override int RawThreadGroups => Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(TLockUC syncPrimitive, IThreadGroupIndex threadGroupIndex) =>
			new BenchmarkWorker[]
			{
				new GeneralLockWorker(syncPrimitive, this, $"{threadGroupIndex}", $"{threadGroupIndex}:A"),
				new GeneralLockWorker(syncPrimitive, this, $"{threadGroupIndex}", $"{threadGroupIndex}:B")
			};
		protected NeighborGeneralBenchmark(IBenchmarkConfiguration test) : base(test) { }
	}
}