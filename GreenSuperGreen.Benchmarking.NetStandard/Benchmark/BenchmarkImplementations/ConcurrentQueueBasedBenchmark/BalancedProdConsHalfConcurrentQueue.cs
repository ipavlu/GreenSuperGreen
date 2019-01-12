using System;
using System.Collections.Concurrent;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class BalancedProdConsHalfConcurrentQueue : Benchmark<ConcurrentQueue<object>>
	{
		protected override ConcurrentQueue<object> SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ConcurrentQueue<object>();
		protected override string NameBase => nameof(BalancedProdConsHalfConcurrentQueue);
		protected override int RawThreadGroups => Environment.ProcessorCount / 4;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ConcurrentQueue<object> syncPrimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ConcurrentQueueWriterWorker(syncPrimitive, this, $"{threadGroupIndex}", $"{threadGroupIndex}:W"),
			new ConcurrentQueueReaderWorker( syncPrimitive, this, $"{threadGroupIndex}", $"{threadGroupIndex}:R")
		};
		public BalancedProdConsHalfConcurrentQueue(IBenchmarkConfiguration test) : base(test) { }
	}
}