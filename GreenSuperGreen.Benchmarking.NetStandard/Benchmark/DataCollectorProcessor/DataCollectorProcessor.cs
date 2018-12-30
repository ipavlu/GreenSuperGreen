using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

namespace GreenSuperGreen.Benchmarking
{
	internal static class DataCollectorProcessor
	{
		public static ImmutableArray<int> GenerateSpinsSet(this int length, double logStepFraction = 6.0)
		{
			double logStep = Math.Pow(2.0, 1.0 / logStepFraction);

			Enumerable
			.Range(0, length)
			.Select(i => (int)(1000.0 * Math.Pow(logStep, i)))
			.ToImmutableArray()
			.AssignOut(out ImmutableArray<int> spinsSet)
			;

			return spinsSet;
		}

		public
		static
		ImmutableArray<IBenchmarkManager>
		GenerateBenchmarksSet(this IEnumerable<KeyValuePair<string, IBenchmarkManager>> enumerable, bool skipDataCollectors = true) =>
		(enumerable ?? Enumerable.Empty<KeyValuePair<string, IBenchmarkManager>>())
		.Select(x => x.Value)
		.GenerateBenchmarksSet(skipDataCollectors)
		;

		public
		static
		ImmutableArray<IBenchmarkManager>
		GenerateBenchmarksSet(this IEnumerable<IBenchmarkManager> enumerable, bool skipDataCollectors = true) =>
		enumerable
		.Where(benchInfo => benchInfo.CollectorSupport == Collector.Collect)
		.ToImmutableArray()
		;


		public static async Task Execute(IPerfCounterCollectorUC perfCollector, ITextWriter textWriter)
		{
			Process.GetCurrentProcess().ProcessName.WriteLine();
			BenchTest.PrintIntro();

			string.Empty.WriteLine();
			string.Empty.WriteLine();

			ImmutableArray<int> spinsSet = 101.GenerateSpinsSet();

			ImmutableArray
			.Create<string>()
			.Add("SpinsSet:")
			.AddRange(spinsSet.Select(x => x.ToString()))
			.Add(string.Empty)
			.Add(string.Empty)
			.Aggregate(string.Empty, (c,n) => string.IsNullOrEmpty(c) ? n : $"{c}{Environment.NewLine}{n}")
			.WriteLine()
			;

			BenchmarkManagersConfig
			.BenchmarkManagers
			.Where(x => x.CollectorSupport == Collector.Collect && !x.IsDataCollector)
			.ToImmutableArray()
			.AssignOut(out ImmutableArray<IBenchmarkManager> candidateBenchmarks)
			;

			ImmutableArray
			.Create<string>()
			.Add("Selected Benchmark Managers:")
			.AddRange(candidateBenchmarks.Select(x => x.Name))
			.Add(string.Empty)
			.Add(string.Empty)
			.Aggregate(string.Empty, (c, n) => string.IsNullOrEmpty(c) ? n : $"{c}{Environment.NewLine}{n}")
			.WriteLine()
			;

			await Task.Delay(2000);

			IBenchmarkManager sequential = BenchmarkManagersConfig.BenchmarkManagers.FirstOrDefault(x => x.IsSequential);
			IBenchmarkConfiguration sequentialConfig = new BenchmarkConfiguration(sequential, perfCollector, BenchmarkGlobalSettings.TestingTimeSpan, 1000000, textWriter);
			await (sequential?.ExecuteBenchmark(sequentialConfig) ?? Task.CompletedTask);


			foreach (int spins in spinsSet)
			{
				foreach (IBenchmarkManager info in candidateBenchmarks)
				{
					await Task.Delay(2000);

					await info.ExecuteBenchmark(new BenchmarkConfiguration(info, perfCollector, BenchmarkGlobalSettings.TestingTimeSpan, spins, textWriter));

					string.Empty.WriteLine();
					string.Empty.WriteLine();
				}
			}

			BenchTest.PrintExit();
		}
	}
}