using System;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class HeavySequential : Benchmark<object>, ISequentialBenchmark
	{
		protected override int RawThreadGroups => Environment.ProcessorCount;
		protected override BenchmarkWorker[] BenchInstanceGenerator(object syncPrimitive, IThreadGroupIndex threadGroupIndex)
			=> new BenchmarkWorker[] { new SequentialWorker(this, $"{threadGroupIndex}", $"{threadGroupIndex}") }
		;
		public HeavySequential(IBenchmarkConfiguration test) : base(test) { }
	}
}