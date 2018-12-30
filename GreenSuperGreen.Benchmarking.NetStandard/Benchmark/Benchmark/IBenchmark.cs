using System;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmark<TSyncPrimitive> : IBenchmark where TSyncPrimitive : class
	{
		TSyncPrimitive CreateOrGetStoredSyncPrimitive(IThreadGroupIndex threadGroupIndex);
		BenchmarkWorker[] CreateBenchInstance(IThreadGroupIndex threadGroupIndex);
	}

	public interface IBenchmark : IBenchmarkConfiguration
	{
		string Name { get; }
		int ThreadGroups { get; }

		Task PerfCollectorTryClearAsync(int delay = 100);
		Task PerfCollectorTryStartAsync(int delay = 100);
		Task PerfCollectorTryStopAsync(int delay = 100);

		Task ExecuteBenchmarkAsync();
	}
}