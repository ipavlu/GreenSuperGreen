using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmark<out TSyncPrimitive> : IBenchmark where TSyncPrimitive : class
	{
		TSyncPrimitive CreateOrGetStoredSyncPrimitive(IThreadGroupIndex threadGroupIndex);
		BenchmarkWorker[] CreateBenchInstance(IThreadGroupIndex threadGroupIndex);
	}

	public interface IBenchmark : IBenchmarkConfiguration
	{


		Task PerfCollectorTryClearAsync(int delay = 100);
		Task PerfCollectorTryStartAsync(int delay = 100);
		Task PerfCollectorTryStopAsync(int delay = 100);

		Task ExecuteBenchmarkAsync();
	}
}