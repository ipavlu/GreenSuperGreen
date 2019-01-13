using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public static class BenchmarkingInitPoint
	{
		private static string TestsMenu { get; } =
		BenchmarkManagersConfig
		.BenchmarkManagers
		.Select(benchmarkManager => $"{benchmarkManager.KeyName}: {benchmarkManager.Name}{Environment.NewLine}")
		.Aggregate(string.Empty, (c, n) => c + n)
		;

		private static void PrintTestMenu() => TestsMenu.WriteLine();

		public static string ProductVersionCopyright { get; } = $"{AssemblyInfo.AssemblyInfo.Product.Product} v{AssemblyInfo.AssemblyInfo.Version.Version} {AssemblyInfo.AssemblyInfo.Copyright.Copyright}";
		public static void PrintIntro() => $"Enjoy! {ProductVersionCopyright}".WriteLine();
		public static void PrintExit() => $"Exit: {ProductVersionCopyright}".WriteLine();

		public static async Task MainAsync(string[] args)
		{
			using (ITextWriter textWriter = new TextWriterReplicatorManager().InstallConsole(DisposeAncestor.Yes))
			{
				await BenchProcessing(args, textWriter);
			}
		}

		private static async Task BenchProcessing(string[] args, ITextWriter textWriter)
		{
			IPerfCounterCollectorUC perfCollector =
			PerfCounterTypeUC.ProcessorTime.PerfCounterCollector(TimeSpan.FromMilliseconds(1000), Process.GetCurrentProcess().ProcessName)
			;

			if (args.Any(str => str.Contains("collect")))
			{
				await DataCollectorBenchmark.CollectData(perfCollector, textWriter);
				return;
			}

			Process.GetCurrentProcess().ProcessName.WriteLine();
			PrintIntro();

			string.Empty.WriteLine();
			string.Empty.WriteLine();

			await ManualSelector(perfCollector, textWriter);

			PrintExit();
		}

		private static async Task ManualSelector(IPerfCounterCollectorUC perfCollector, ITextWriter textWriter)
		{
			while (true)
			{
				PrintTestMenu();

				$"Select tests or [Exit]/[Quit]:{Environment.NewLine}".WriteLine();

				(System.Console.ReadLine() ?? string.Empty)
				.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
				.Select(str => str.ToLower())
				.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
				.Distinct()
				.ToImmutableHashSet()
				.AssignOut(out ImmutableHashSet<string> benchmarkSelector)
				.Select(BenchmarkManagersConfig.GetBenchmarkManager)
				.Where(benchmarkManager => benchmarkSelector != null)
				.ToImmutableArray()
				.AssignOut(out ImmutableArray<IBenchmarkManager> selectedBenchmarkManagers)
				;

				if (benchmarkSelector.Contains("exit") || benchmarkSelector.Contains("quit"))
				{
					"Requested Exit".WriteLine();
					return;
				}

				int spins = 0;


				if (	selectedBenchmarkManagers
					    .Where(bManager => bManager.IsDataCollector)
					    .ToImmutableArray()
					    .AssignOut(out ImmutableArray<IBenchmarkManager> dataCollector)
					    .Length > 0)
				{
					selectedBenchmarkManagers = dataCollector;
				}
				else
				{
					$"Select spin time:{Environment.NewLine}".WriteLine();
					string spinText = System.Console.ReadLine() ?? string.Empty;
					if (!int.TryParse(spinText, out spins) || spins < 0) continue;
				}

				foreach (IBenchmarkManager test in selectedBenchmarkManagers)
				{
					string.Empty.WriteLine();

					await test.ExecuteBenchmark(new BenchmarkConfiguration(test, perfCollector, BenchmarkGlobalSettings.TestingTimeSpan, spins, textWriter));

					string.Empty.WriteLine();
					await Task.Delay(2000);
				}
			}
		}
	}
}
