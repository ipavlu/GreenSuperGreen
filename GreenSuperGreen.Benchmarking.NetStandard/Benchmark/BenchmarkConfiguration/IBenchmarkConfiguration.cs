using System;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkConfiguration
	{
		IBenchmarkManager BenchmarkManager { get; }
		IPerfCounterCollectorUC PerfCollector { get; }
		TimeSpan TimeSpan { get; }
		long Spins { get; }
		ITextWriter TextWriter { get; }
	}
}