using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkWorker
	{
		long ElapsedMilliseconds { get; }
		int Iterations { get; }
		string Pair { get; }
		double ThroughputPerMillisecond { get; }

		Task RunProcessing();
	}
}