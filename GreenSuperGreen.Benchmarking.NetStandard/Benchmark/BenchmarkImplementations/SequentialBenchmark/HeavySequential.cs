namespace GreenSuperGreen.Benchmarking
{
	public class HeavySequential : Benchmark<object>, ISequentialBenchmark
	{
		protected override int RawThreadGroups => 1;
		protected override BenchmarkWorker[] BenchInstanceGenerator(object syncprimitive, IThreadGroupIndex threadGroupIndex)
			=> new BenchmarkWorker[] { new SequentialWorker(this, string.Empty) }
		;
		public HeavySequential(IBenchmarkConfiguration test) : base(test) { }
	}
}