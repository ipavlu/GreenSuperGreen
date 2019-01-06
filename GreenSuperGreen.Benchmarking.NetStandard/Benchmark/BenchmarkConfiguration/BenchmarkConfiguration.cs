using System;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

// ReSharper disable ConstantNullCoalescingCondition
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class BenchmarkConfiguration : IBenchmarkConfiguration
	{
		private readonly string _nameBase;
		public IBenchmarkManager BenchmarkManager { get; }
		public IPerfCounterCollectorUC PerfCollector { get; }
		protected virtual string NamePrefix => string.Empty;
		protected virtual string NameBase => _nameBase ?? GetType().Name;
		protected virtual string NamePostfix => string.Empty;
		public string Name => $"{NamePrefix}{NameBase}{NamePostfix}";

		protected virtual int RawThreadGroups { get; } = 0;
		public int ThreadGroups => Math.Max(RawThreadGroups, 1);

		//public int ThreadGroups { get; }
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
			_nameBase = benchmarkManager.Name;
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