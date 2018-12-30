using System;
using System.Collections.Concurrent;

namespace GreenSuperGreen.Benchmarking
{
	public class BalancedProdConsHalfConcurrentQueue : Benchmark<ConcurrentQueue<object>>
	{
		protected override ConcurrentQueue<object> SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ConcurrentQueue<object>();
		protected override string NameBase => nameof(BalancedProdConsHalfConcurrentQueue);
		protected override int RawThreadGroups => Environment.ProcessorCount / 4;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ConcurrentQueue<object> syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ConcurrentQueueWriterWorker(syncprimitive, this, $"{threadGroupIndex}:W"),
			new ConcurrentQueueReaderWorker( syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public BalancedProdConsHalfConcurrentQueue(IBenchmarkConfiguration test) : base(test) { }
	}
}