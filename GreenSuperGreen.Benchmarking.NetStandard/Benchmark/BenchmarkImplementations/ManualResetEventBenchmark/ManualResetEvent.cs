using System;
using System.Threading;

namespace GreenSuperGreen.Benchmarking
{
	public class ManualResetEvent1 : Benchmark<ManualResetEvent>
	{
		protected override ManualResetEvent SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ManualResetEvent(false);
		protected override string NameBase => nameof(ManualResetEvent1);
		protected override int RawThreadGroups => 1;//Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ManualResetEvent syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ManualResetEventSetterWorker(syncprimitive, this, $"{threadGroupIndex}:S"),
			new ManualResetEventReSetterWorker(syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public ManualResetEvent1(IBenchmarkConfiguration test) : base(test) { }
	}

	public class ManualResetEvent2 : Benchmark<ManualResetEvent>
	{
		protected override ManualResetEvent SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ManualResetEvent(false);
		protected override string NameBase => nameof(ManualResetEvent2);
		protected override int RawThreadGroups => 2;//Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ManualResetEvent syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ManualResetEventSetterWorker(syncprimitive, this, $"{threadGroupIndex}:S"),
			new ManualResetEventReSetterWorker(syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public ManualResetEvent2(IBenchmarkConfiguration test) : base(test) { }
	}

	public class ManualResetEvent4 : Benchmark<ManualResetEvent>
	{
		protected override ManualResetEvent SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ManualResetEvent(false);
		protected override string NameBase => nameof(ManualResetEvent4);
		protected override int RawThreadGroups => 4;//Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ManualResetEvent syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ManualResetEventSetterWorker(syncprimitive, this, $"{threadGroupIndex}:S"),
			new ManualResetEventReSetterWorker(syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public ManualResetEvent4(IBenchmarkConfiguration test) : base(test) { }
	}

	public class ManualResetEventHalf : Benchmark<ManualResetEvent>
	{
		protected override ManualResetEvent SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => new ManualResetEvent(false);
		protected override string NameBase => nameof(ManualResetEventHalf);
		protected override int RawThreadGroups => Environment.ProcessorCount / 2;
		protected override BenchmarkWorker[] BenchInstanceGenerator(ManualResetEvent syncprimitive, IThreadGroupIndex threadGroupIndex) =>
		new BenchmarkWorker[]
		{
			new ManualResetEventSetterWorker(syncprimitive, this, $"{threadGroupIndex}:S"),
			new ManualResetEventReSetterWorker(syncprimitive, this, $"{threadGroupIndex}:R")
		};
		public ManualResetEventHalf(IBenchmarkConfiguration test) : base(test) { }
	}
}