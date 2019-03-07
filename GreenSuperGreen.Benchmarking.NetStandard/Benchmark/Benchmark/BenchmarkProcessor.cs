using System;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.Reporting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Benchmarking
{
	public static class BenchmarkProcessor
	{
		public static async Task Execute<TSyncPrimitive>(IBenchmark<TSyncPrimitive> benchmark)
			where TSyncPrimitive : class
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.NamesAsValues()
			.ToString()
			.WriteLine()
			;

			Enumerable
			.Range(0, benchmark.ThreadGroups)
			.Select(ThreadGroupIndex.New)
			.Select(benchmark.CreateBenchInstance)
			.SelectMany(xws => xws)
			.ToArray()
			.AssignOut(out BenchmarkWorker[] workers)
			;

			Task[] tasks = new Task[workers.Length];
			for (int i = 0; i < workers.Length; ++i) tasks[i] = workers[i].RunProcessing();

			await benchmark.PerfCollectorTryClearAsync();
			await benchmark.PerfCollectorTryStartAsync();
			await Task.WhenAll(tasks);
			await benchmark.PerfCollectorTryStopAsync();

			workers
			.Select(w => ReportUC
						.New<BenchInfoNames>(ReportTypeUC.CVS)
						.Report(benchmark.Name, BenchInfoNames.Name)
						.Report(w.Pair, BenchInfoNames.ThreadPair)
						.Report($"{workers.Length}", BenchInfoNames.Threads)
						.Report($"{w.ElapsedMilliseconds}", BenchInfoNames.Time_ms)
						.Report($"{w.Iterations}", BenchInfoNames.Iterations)
						.Report($"{w.Spins}", BenchInfoNames.Spins)
						.Report($"{w.ThroughputPerMillisecond:0.000}", BenchInfoNames.Throughput_ms)
						.Report(w.ResourceName, BenchInfoNames.ResourceName)
						.ToString())
			.Aggregate(string.Empty, (c,n) => string.IsNullOrEmpty(c) ? n : $"{c}{Environment.NewLine}{n}")
			.WriteLine()
			;

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.NamesAsValues()
			.ToString()
			.WriteLine()
			;

			long totalSpins = workers.Select(w => w.Spins).Aggregate(0L, (c, n) => c + n);
			long avgSpins = (int)(totalSpins / Math.Max(workers.LongLength, 1L));

			long totalTime = workers.Select(w => w.ElapsedMilliseconds).Aggregate(0L, (c, n) => c + n);
			long avgTime = totalTime / Math.Max(workers.LongLength, 1L);
			//long medianTime = (long)workers.Select(w => (double) w.ElapsedMilliseconds).Median();

			long totalIterations = workers.Select(w => (long)w.Iterations).Aggregate(0L, (c, n) => c + n);
			long avgIterations = totalIterations / Math.Max(workers.Length, 1L);
			long medianIterations = (long)workers.Select(w => (double)w.Iterations).Median();

			double totalCPU = benchmark.PerfCollector.Select(cpu => cpu).Aggregate(0.0, (c, n) => c + n);
			double avgCPU = totalCPU / Math.Max(benchmark.PerfCollector.Count(), 1L);
			double medianCPU = benchmark.PerfCollector.Median();

			double totalThroughput = workers.Select(w => w.ThroughputPerMillisecond).Aggregate(0.0, (c, n) => c + n);
			double avgThroughput = totalThroughput / Math.Max(workers.Length, 1L);
			double medianThroughput = workers.Select(w => w.ThroughputPerMillisecond).Median();

			benchmark
			.PerfCollector
			.Select(cpu =>	ReportUC
							.New<BenchInfoNames>(ReportTypeUC.CVS)
							.Report(benchmark.Name, BenchInfoNames.Name)
							.Report($"{cpu}", BenchInfoNames.CPU)
							.Report($"{workers.Length}", BenchInfoNames.Threads)
							.Report($"{avgTime}", BenchInfoNames.Time_ms)
							.Report($"{avgSpins}", BenchInfoNames.Spins)
							.Report($"{avgIterations}", BenchInfoNames.AvgIterations)
							.ToString())
			.Aggregate(string.Empty, (c, n) => string.IsNullOrEmpty(c) ? n : $"{c}{Environment.NewLine}{n}")
			.WriteLine()
			;

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.NamesAsValues()
			.ToString()
			.WriteLine()
			;

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.Report(benchmark.Name, BenchInfoNames.Name)
			.Report($"{workers.Length}", BenchInfoNames.Threads)
			.Report($"{totalIterations}", BenchInfoNames.TotalIterations)
			.Report($"{avgTime}", BenchInfoNames.Time_ms)
			.Report($"{avgIterations}", BenchInfoNames.AvgIterations)
			.Report($"{medianIterations}", BenchInfoNames.MedianIterations)
			.Report($"{avgSpins}", BenchInfoNames.Spins)
			.Report($"{avgCPU:0}", BenchInfoNames.AvgCPU)
			.Report($"{medianCPU:0}", BenchInfoNames.MedianCPU)
			.Report($"{avgThroughput:0.000}", BenchInfoNames.AvgThroughput_ms)
			.Report($"{medianThroughput:0.000}", BenchInfoNames.MedianThroughput_ms)
			.ToString()
			.WriteLine()
			;
		}
	}
}