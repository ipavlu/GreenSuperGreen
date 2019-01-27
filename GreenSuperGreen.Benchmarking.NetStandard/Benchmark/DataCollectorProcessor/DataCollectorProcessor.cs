using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.Reporting;
using GreenSuperGreen.TextWriterReplication;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

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
			BenchmarkingInitPoint.PrintIntro();

			string.Empty.WriteLine().WriteLine();

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.NamesAsValues()
			.ToString()
			.WriteLine()
			;

			string procName = ProcessorName();
			string osName = OperatingSystemName();

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.Report("SystemInformation:", BenchInfoNames.Name)
			.Report(procName, BenchInfoNames.NameCPU)
			.Report(Environment.ProcessorCount.ToString(), BenchInfoNames.CoresCPU)
			.Report(Environment.OSVersion.ToString(), BenchInfoNames.VersionOS)
			.Report(osName, BenchInfoNames.NameOS)
			.ToString()
			.WriteLine()
			;

			string.Empty.WriteLine().WriteLine();


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

			//IBenchmarkManager sequential = BenchmarkManagersConfig.BenchmarkManagers.FirstOrDefault(x => x.IsSequential);
			//IBenchmarkConfiguration sequentialConfig = new BenchmarkConfiguration(sequential, perfCollector, BenchmarkGlobalSettings.TestingTimeSpan, 1000000, textWriter);
			//await (sequential?.ExecuteBenchmark(sequentialConfig) ?? Task.CompletedTask);

			string.Empty.WriteLine().WriteLine();

			foreach (int spins in spinsSet)
			{
				foreach (IBenchmarkManager info in candidateBenchmarks)
				{
					await Task.Delay(2000);

					await info.ExecuteBenchmark(new BenchmarkConfiguration(info, perfCollector, BenchmarkGlobalSettings.TestingTimeSpan, spins, textWriter));

					string.Empty.WriteLine().WriteLine();
				}
			}

			BenchmarkingInitPoint.PrintExit();
		}


		public static string ProcessorName()
		{
			try
			{
				using (Process process = new Process())
				{
					process.StartInfo.FileName = "cmd.exe";
					process.StartInfo.Arguments = "/c wmic CPU get NAME";
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;
					process.Start();
					//* Read the output (or the error)
					string output = process.StandardOutput.ReadToEnd();
					string err = process.StandardError.ReadToEnd();
					process.WaitForExit();

					output = output.Replace("\r", string.Empty);
					output = output.Replace("\n", string.Empty);
					output = output.Replace("\t", string.Empty);
					output = output.Replace("\v", string.Empty);
					output = output.Replace("Name", string.Empty);
					output = output.Trim();

					return output;
				}
			}
			catch (Exception)
			{
				return "Information retrieval has failed!";
			}
		}

		public static string OperatingSystemName()
		{
			try
			{
				using (Process process = new Process())
				{
					process.StartInfo.FileName = "cmd.exe";
					process.StartInfo.Arguments = "/c wmic OS get NAME";
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;
					process.Start();
					//* Read the output (or the error)
					string output = process.StandardOutput.ReadToEnd();
					string err = process.StandardError.ReadToEnd();
					process.WaitForExit();

					output = output.Replace("\r", string.Empty);
					output = output.Replace("\n", string.Empty);
					output = output.Replace("\t", string.Empty);
					output = output.Replace("\v", string.Empty);
					output = output.Replace("Name", string.Empty);
					output = output.Split(new[] {"|"}, StringSplitOptions.None).FirstOrDefault() ?? string.Empty;
					output = output.Trim();

					return output;
				}
			}
			catch (Exception)
			{
				return "Information retrieval has failed!";
			}
		}
	}
}