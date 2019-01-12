using System;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkDataConfiguration
	{
		string Name { get; }
		int ThreadGroups { get; }
		TimeSpan TimeSpan { get; }
		long Spins { get; }
	}

	public interface IBenchmarkConfiguration : IBenchmarkDataConfiguration
	{
		IBenchmarkManager BenchmarkManager { get; }
		IPerfCounterCollectorUC PerfCollector { get; }
		ITextWriter TextWriter { get; }
	}
}