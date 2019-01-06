using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkWorkerData
	{
		long ElapsedMilliseconds { get; }
		int Iterations { get; }
		double ThroughputPerMillisecond { get; }

		string ResourceName { get; }

		string Pair { get; }
	}
	public interface IBenchmarkWorker : IBenchmarkWorkerData
	{
		Task RunProcessing();
	}
}