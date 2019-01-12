using System;
using System.Threading.Tasks;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class BenchmarkManager
	{
		public static TimeSpan TestingTimeSpan { get; } = TimeSpan.FromSeconds(10);
		public static int TestingSpins { get; } = 50000;

	}
	public class BenchmarkManager<TBenchmark> : BenchmarkManager, IBenchmarkManager where TBenchmark : class, IBenchmark
	{
		public int Id { get; }
		public string Name { get; } = typeof(TBenchmark).Name;
		public string KeyName { get; }
		public Collector CollectorSupport { get; }

		public bool IsSequential { get; } = typeof(TBenchmark).GetInterfaces().Any(type => type == typeof(ISequentialBenchmark));
		public bool IsDataCollector { get; } = typeof(TBenchmark).GetInterfaces().Any(type => type == typeof(IDataCollectorBenchmark));

		private IBenchmarkFactory BenchmarkFactory { get; } = new BenchmarkFactory<TBenchmark>();

		public BenchmarkManager(int id, string keyName, Collector collectorSupport)
		{
			Id = id;
			KeyName = keyName ?? throw new ArgumentNullException(nameof(keyName));
			CollectorSupport = collectorSupport;
		}
		
		public Task ExecuteBenchmark(IBenchmarkConfiguration test) =>
		BenchmarkFactory
		?.GetBenchmark(test)
		?.ExecuteBenchmarkAsync() ?? Task.CompletedTask
		;
	}
}