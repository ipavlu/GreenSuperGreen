using System;
using System.Collections.Concurrent;

namespace GreenSuperGreen.Benchmarking
{
	public class BalancedProdConsConcurrentQueue : Benchmark<ConcurrentQueue<object>>
	{
		protected override ConcurrentQueue<object> SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ConcurrentQueue<object>();
		protected override string NameBase => nameof(BalancedProdConsConcurrentQueue);
		protected override int RawThreadGroups => Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ConcurrentQueue<object> syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ConcurrentQueueWriterWorker(syncprimitive, this,  $"{threadGroupIndex}:W"),
			new ConcurrentQueueReaderWorker(syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public BalancedProdConsConcurrentQueue(IBenchmarkConfiguration test) : base(test) { }
	}
}