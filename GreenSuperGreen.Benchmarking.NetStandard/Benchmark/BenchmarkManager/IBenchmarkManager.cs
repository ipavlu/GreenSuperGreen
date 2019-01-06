using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkManager
	{
		int Id { get; }
		string Name { get; }
		string KeyName { get; }
		Collector CollectorSupport { get; }

		bool IsSequential { get; }
		bool IsDataCollector { get; }

		Task ExecuteBenchmark(IBenchmarkConfiguration test);
	}
}