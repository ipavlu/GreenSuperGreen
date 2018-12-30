using System;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

namespace GreenSuperGreen.Benchmarking
{
	public class BenchmarkConfiguration : IBenchmarkConfiguration
	{
		public IBenchmarkManager BenchmarkManager { get; }
		public IPerfCounterCollectorUC PerfCollector { get; }
		public TimeSpan TimeSpan { get; }
		public long Spins { get; }
		public ITextWriter TextWriter { get; }


		public BenchmarkConfiguration(
			IBenchmarkManager benchmarkManager,
			IPerfCounterCollectorUC perfCollector,
			TimeSpan timeSpan,
			long spins,
			ITextWriter textWriter = null)
		{
			BenchmarkManager = benchmarkManager ?? throw new ArgumentNullException(nameof(benchmarkManager));
			PerfCollector = perfCollector ?? throw new ArgumentNullException(nameof(perfCollector));
			TimeSpan = timeSpan;
			Spins = spins;
			TextWriter = textWriter;
		}

		public BenchmarkConfiguration(IBenchmarkConfiguration benchmarkConfiguration)
		{
			BenchmarkManager = benchmarkConfiguration?.BenchmarkManager ?? throw new ArgumentNullException(nameof(benchmarkConfiguration.BenchmarkManager));
			PerfCollector = benchmarkConfiguration?.PerfCollector ?? throw new ArgumentNullException(nameof(benchmarkConfiguration.PerfCollector));
			TimeSpan = benchmarkConfiguration?.TimeSpan ?? BenchmarkGlobalSettings.TestingTimeSpan;
			Spins = benchmarkConfiguration?.Spins ?? BenchmarkGlobalSettings.TestingSpins;
			TextWriter = benchmarkConfiguration?.TextWriter;
		}
	}
}