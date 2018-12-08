using System;
using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.Queues;
using GreenSuperGreen.Reporting;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public enum BenchInfoNames
	{
		Name = 0,
		ThreadPair = 1,
		Threads = 2,
		Time_ms = 3,
		Iterations = 4,
		AvgIterations = 5,
		MedianIterations = 6,
		TotalIterations = 7,
		Spins = 8,
		CPU = 9,
		AvgCPU = 10,
		MedianCPU = 11,
		Throughput_ms = 12,
		AvgThroughput_ms = 13,
		MedianThroughput_ms = 14,
	}

	public class BenchInfo
	{
		public static TimeSpan TestingTimeSpan { get; } = TimeSpan.FromSeconds(10);
		public string Name { get; set; }
		public string KeyName { get; set; }
		public bool CollectorSupport { get; set; } = true;
		public Func<BenchInfo, IPerfCounterCollectorUC, TimeSpan, int, Task> Task { get; set; }
		public Task Test(BenchInfo bi, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spins) => Task?.Invoke(bi, perfCollector, ts, spins) ?? System.Threading.Tasks.Task.CompletedTask;
	}

	public static class BenchTest
	{
		public static Action<string> WriteContext = System.Console.WriteLine;
		public static void WriteLine(this string msg) => WriteContext?.Invoke(msg);

		private static IDictionary<string, BenchInfo> Tests { get; } = new[]
		{
			new BenchInfo() {KeyName = "c", CollectorSupport = false, Name =  nameof(DataCollector), Task = (bi,pc,t,a) => DataCollector(pc)},
			new BenchInfo() {KeyName = "0", CollectorSupport = false, Name =  nameof(SyncPrimitivesBenchmark.HeavySequential), Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavySequential(bi,pc,t,a,1)},

			new BenchInfo() {KeyName = "q", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyLockUC(bi,pc,t,a,Environment.ProcessorCount)},
			new BenchInfo() {KeyName = "w", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyMonitorLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyMonitorLockUC(bi,pc,t,a,Environment.ProcessorCount)},
			new BenchInfo() {KeyName = "e", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavySpinLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavySpinLockUC(bi,pc,t,a,Environment.ProcessorCount)},
			new BenchInfo() {KeyName = "r", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyTicketSpinLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyTicketSpinLockUC(bi,pc,t,a,Environment.ProcessorCount)},
			new BenchInfo() {KeyName = "t", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			new BenchInfo() {KeyName = "y", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborMonitorLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborMonitorLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			new BenchInfo() {KeyName = "u", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborSpinLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborSpinLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			new BenchInfo() {KeyName = "i", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborTicketSpinLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborTicketSpinLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			new BenchInfo() {KeyName = "o", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyAsyncLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyAsyncLockUC(bi,pc,t,a,Environment.ProcessorCount)},
			new BenchInfo() {KeyName = "p", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborAsyncLockUC)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborAsyncLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},

			//new BenchInfo() {KeyName = "1", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.ManualResetEvent)} distinct producer/consumer pairs: 1", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().ManualResetEvent(bi,pc,t,a,1)},
			//new BenchInfo() {KeyName = "2", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.ManualResetEvent)} distinct producer/consumer pairs: 2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().ManualResetEvent(bi,pc,t,a,2)},
			//new BenchInfo() {KeyName = "3", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.ManualResetEvent)} distinct producer/consumer pairs: 4", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().ManualResetEvent(bi,pc,t,a,4)},
			//new BenchInfo() {KeyName = "4", CollectorSupport = true, Name =  $"{nameof(SyncPrimitivesBenchmark.ManualResetEvent)} distinct producer/consumer pairs: cores/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().ManualResetEvent(bi,pc,t,a,Environment.ProcessorCount/2)},

			//new BenchInfo() {KeyName = "a", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			//new BenchInfo() {KeyName = "s", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyMonitorLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyMonitorLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			//new BenchInfo() {KeyName = "d", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavySpinLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavySpinLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			//new BenchInfo() {KeyName = "f", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyTicketSpinLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyTicketSpinLockUC(bi,pc,t,a,Environment.ProcessorCount/2)},
			//new BenchInfo() {KeyName = "g", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborLockUC(bi,pc,t,a,Environment.ProcessorCount/4)},
			//new BenchInfo() {KeyName = "h", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborMonitorLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborMonitorLockUC(bi,pc,t,a,Environment.ProcessorCount/4)},
			//new BenchInfo() {KeyName = "j", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborSpinLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborSpinLockUC(bi,pc,t,a,Environment.ProcessorCount/4)},
			//new BenchInfo() {KeyName = "k", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.NeighborTicketSpinLockUC)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().NeighborTicketSpinLockUC(bi,pc,t,a,Environment.ProcessorCount/4)},

			//new BenchInfo() {KeyName = "m", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyConcurrentQueue)}", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyConcurrentQueue(bi,pc,t,a,Environment.ProcessorCount/2)},
			//new BenchInfo() {KeyName = "n", CollectorSupport = false, Name =  $"{nameof(SyncPrimitivesBenchmark.HeavyConcurrentQueue)}/2", Task = (bi,pc,t,a) => new SyncPrimitivesBenchmark().HeavyConcurrentQueue(bi,pc,t,a,Environment.ProcessorCount/4)},

		}.ToDictionary(info => info.KeyName, info => info);

		public static async Task MainAsync(string[] args)
		{
			IPerfCounterCollectorUC perfCollector = PerfCounterTypeUC.ProcessorTime.PerfCounterCollector(TimeSpan.FromMilliseconds(1000), Process.GetCurrentProcess().ProcessName);

			if (args.Any(str => str.Contains("collect")))
			{
				await DataCollector(perfCollector);
				return;
			}

			WriteContext = System.Console.WriteLine;

			Process.GetCurrentProcess().ProcessName.WriteLine();
			"Enjoy! ipavlu 2017".WriteLine();
			string.Empty.WriteLine();
			string.Empty.WriteLine();

			await Tests.TestsExecute(perfCollector);

			"done".WriteLine();
		}

		private static void TestsPrint(this IDictionary<string, BenchInfo> tests)
		{
			(tests ?? Enumerable.Empty<KeyValuePair<string, BenchInfo>>())
			.Select(kvp => $"{kvp.Key}: {kvp.Value.Name}{Environment.NewLine}")
			.Aggregate(string.Empty, (c, n) => c + n)
			.WriteLine()
			;
		}

		private static async Task TestsExecute(this IDictionary<string, BenchInfo> testsInfo, IPerfCounterCollectorUC perfCollector)
		{
			while (true)
			{
				testsInfo.TestsPrint();

				$"Select tests[X = Exit]:{Environment.NewLine}".WriteLine();

				Dictionary<string, string> tests =
				(System.Console.ReadLine() ?? string.Empty)
				.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
				.Select(str => str.ToLower())
				.Where(str => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
				.Distinct()
				.ToDictionary(str => str)
				;

				BenchInfo[] testInfos =
				Tests
				.Select(kvp => kvp.Value)
				.Where(x => !tests.Any() || tests.ContainsKey(x.KeyName.ToLower()))
				.ToArray()
				;

				if (tests.ContainsKey("x") || tests.ContainsKey("exit") || tests.ContainsKey("quit"))
				{
					"Exit".WriteLine();
					return;
				}

				$"Select spin time:{Environment.NewLine}".WriteLine();
				string spinText = System.Console.ReadLine() ?? string.Empty;
				if (!int.TryParse(spinText, out var spin) || spin < 0) continue;

				foreach (BenchInfo test in testInfos)
				{
					string.Empty.WriteLine();
					await test.Test(test, perfCollector, BenchInfo.TestingTimeSpan, spin);
					string.Empty.WriteLine();
					await Task.Delay(2000);
				}
			}
		}

		public static double Median(this IEnumerable<double> ie)
		{
			double[] sortedNumbers = ie.OrderBy(n => n).ToArray();
			int halfIndex = sortedNumbers.Length / 2;
			double median = (sortedNumbers.Length % 2) == 0
			? (sortedNumbers[halfIndex] + sortedNumbers[halfIndex - 1]) / 2.0
			: sortedNumbers[halfIndex]
			;
			return median;
		}

		private enum priority { top };
		private static IPriorityQueueNotifierUC<priority,string> NotificationQueue { get; } = new PriorityQueueNotifierUC<priority, string>(new [] {priority.top});

		private static async Task FileRecorder()
		{
			using (StreamWriter fs = File.AppendText("UnifiedConcurrencyReport.txt"))
			{
				while (true)
				{
					string msg;
					await NotificationQueue.EnqueuedItemsAsync();
					if (!NotificationQueue.TryDequeu(out msg) || msg == null) break;
					await fs.WriteLineAsync(msg);
					await fs.FlushAsync();
				}
				fs.Close();
			}
		}

		public static async Task DataCollector(IPerfCounterCollectorUC perfCollector)
		{
			Task recorder = Task.Run(FileRecorder);

			WriteContext = msg =>
			{
				NotificationQueue.Enqueue(priority.top, msg);
				System.Console.WriteLine(msg ?? string.Empty);
			};

			Process.GetCurrentProcess().ProcessName.WriteLine();
			"Enjoy! ipavlu 2017".WriteLine();

			await Task.Delay(2000);
			BenchInfo sequential = Tests[0.ToString()];
			await sequential.Test(sequential, perfCollector, BenchInfo.TestingTimeSpan, 1000000);
			string.Empty.WriteLine();
			string.Empty.WriteLine();

			double steps = 6;
			double logStep = Math.Pow(2.0, 1.0 / steps);

			int[] spins =
			Enumerable
			.Range(0, 101)
			.Select(i => (int)(1000.0 * Math.Pow(logStep, i)))
			.ToArray()
			;

			foreach (int spin in spins)
			{
				foreach (BenchInfo info in Tests.Where(x => x.Value.CollectorSupport).Select(x => x.Value))
				{
					await Task.Delay(2000);
					await info.Test(info, perfCollector, BenchInfo.TestingTimeSpan, spin);
					string.Empty.WriteLine();
					string.Empty.WriteLine();
				}
			}

			((string)null).WriteLine();
			await recorder;
			WriteContext = System.Console.WriteLine;
		}
	}

	public abstract class BenchInstance
	{
		private Stopwatch StopWatchTimer { get; set; } = new Stopwatch();

		public IPerfCounterCollectorUC PerfCollector { get; }
		public int Spin { get; }
		public TimeSpan TestingTimeSpan { get; }
		public long ElapsedMilliseconds { get; private set; }
		public int Iterations { get; set; }
		public string Pair { get; set; }
		public double ThroughputPerMillisecond => ElapsedMilliseconds / (double)(Iterations == 0 ? 1 : Iterations);

		protected BenchInstance(IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, string pair = null)
		{
			PerfCollector = perfCollector;
			TestingTimeSpan = ts;
			Spin = spin;
			Pair = pair ?? string.Empty;
		}

		protected void WastingTime()
		{
			// ReSharper disable once EmptyForStatement
			// ReSharper disable once SuggestVarOrType_BuiltInTypes
			for (int t = 0; t < Spin; ++t) { }
		}

		protected virtual async Task BenchTarget() { await Task.CompletedTask; throw new NotImplementedException(); }

		public Task RunBenchProcessing() => Task.Run(BenchProcessing);

		protected async Task BenchProcessing()
		{
			StopWatchTimer.Start();
			TimeSpan? next = null;
			TimeSpan now;
			do
			{
				await BenchTarget();
				Iterations++;

				now = StopWatchTimer.Elapsed;
				next = now > next ? now + PerfCollector.TryRead(): next ?? (now + PerfCollector.TryRead());
			} while (now < TestingTimeSpan);

			StopWatchTimer.Stop();
			ElapsedMilliseconds = StopWatchTimer.ElapsedMilliseconds;
			StopWatchTimer = null;
		}
	}

	public class SequentialWorker : BenchInstance
	{
		public SequentialWorker(IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, string pair = null) : base(perfCollector, ts, spin, pair) { }

		protected override async Task BenchTarget()
		{
			WastingTime();
			await Task.CompletedTask;
		}
	}

	public class SimpleLockWorker : BenchInstance
	{
		private ILockUC Lock { get; }
		public SimpleLockWorker(IPerfCounterCollectorUC perfCollector, ILockUC syncPrimitive, TimeSpan ts, int spin, string pair = null) : base(perfCollector, ts, spin, pair) { Lock = syncPrimitive; }

		protected override async Task BenchTarget()
		{
			using (Lock.Enter())
			{
				WastingTime();
			}
			await Task.CompletedTask;
		}
	}

	public class AsyncLockWorker : BenchInstance
	{
		private IAsyncLockUC Lock { get; }
		public AsyncLockWorker(IPerfCounterCollectorUC perfCollector, IAsyncLockUC syncPrimitive, TimeSpan ts, int spin, string pair = null) : base(perfCollector, ts, spin, pair) { Lock = syncPrimitive; }

		protected override async Task BenchTarget()
		{
			using (await Lock.Enter())
			{
				WastingTime();
			}
		}
	}

	public class ManualResetEventSetterWorker : BenchInstance
	{
		private ManualResetEvent mre { get; }
		public ManualResetEventSetterWorker(IPerfCounterCollectorUC perfCollector, ManualResetEvent syncPrimitive, TimeSpan ts, int spin, string pair = null) : base(perfCollector, ts, spin, pair) { mre = syncPrimitive; }

		protected override async Task BenchTarget()
		{
			WastingTime();
			mre.Set();
			await Task.CompletedTask;
		}
	}

	public class ManualResetEventReSetterWorker : BenchInstance
	{
		private ManualResetEvent mre { get; }
		public ManualResetEventReSetterWorker(IPerfCounterCollectorUC perfCollector, ManualResetEvent syncPrimitive, TimeSpan ts, int spin, string pair = null) : base(perfCollector, ts, spin, pair) { mre = syncPrimitive; }

		protected override async Task BenchTarget()
		{
			mre.Reset();
			await Task.CompletedTask;
		}
	}
	public class ConcurrentQueueReaderWorker : BenchInstance
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueReaderWorker(	ConcurrentQueue<object> queue,
											IPerfCounterCollectorUC perfCollector,
											TimeSpan ts,
											int spin,
											string pair = null)
		:	base(perfCollector, ts, spin, pair)
		{
			Queue = queue;
		}

		protected override async Task BenchTarget()
		{
			object rslt;
			while (!Queue.TryDequeue(out rslt)) { }
			await Task.CompletedTask;
		}
	}

	public class ConcurrentQueueWriterWorker : BenchInstance
	{
		private ConcurrentQueue<object> Queue { get; }

		public ConcurrentQueueWriterWorker(	ConcurrentQueue<object> queue,
											IPerfCounterCollectorUC perfCollector,
											TimeSpan ts,
											int spin,
											string pair = null)
		: base(perfCollector, ts, spin, pair)
		{
			Queue = queue;
		}

		protected override async Task BenchTarget()
		{
			Queue.Enqueue(null);
			await Task.CompletedTask;
		}
	}

	public class SyncPrimitivesBenchmark
	{
		public
		async Task
		GeneralBenchmark<TSyncPrimitive>
		(	IPerfCounterCollectorUC perfCollector,
			string name,
			int items,
			Func<int,TSyncPrimitive> createSyncPrimitive,
			Func<int,IPerfCounterCollectorUC, TSyncPrimitive, BenchInstance[]> createBenchInstance)
		{
			items = Math.Max(items, 1);

			ReportUC
			.New<BenchInfoNames>(ReportTypeUC.CVS)
			.NamesAsValues()
			.ToString()
			.WriteLine()
			;

			BenchInstance[] workers =
			Enumerable
			.Range(0, items)
			.Select(createSyncPrimitive)
			.Select((sync, i) => createBenchInstance(i, perfCollector, sync))
			.SelectMany(xws => xws)
			.ToArray()
			;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < workers.Length; ++i) tasks.Add(workers[i].RunBenchProcessing());

			while (perfCollector.TryClear() != true) { }
			while (perfCollector.TryStart() != true) { }
			await Task.WhenAll(tasks);
			while (perfCollector.TryStop() != true) { }

			workers
			.Select(w => ReportUC
						.New<BenchInfoNames>(ReportTypeUC.CVS)
						.Report(name, BenchInfoNames.Name)
						.Report(w.Pair, BenchInfoNames.ThreadPair)
						.Report($"{workers.Length}", BenchInfoNames.Threads)
						.Report($"{w.ElapsedMilliseconds}", BenchInfoNames.Time_ms)
						.Report($"{w.Iterations}", BenchInfoNames.Iterations)
						.Report($"{w.Spin}", BenchInfoNames.Spins)
						.Report($"{w.ThroughputPerMillisecond:0.000}", BenchInfoNames.Throughput_ms)
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

			long totalSpins = workers.Select(w => (long)w.Spin).Aggregate(0L, (c, n) => c + n);
			int avgSpins = (int)(totalSpins / Math.Max(workers.Length, 1));

			long totalTime = workers.Select(w => w.ElapsedMilliseconds).Aggregate(0L, (c, n) => c + n);
			long avgTime = totalTime / Math.Max(workers.Length, 1);
			//long medianTime = (long)workers.Select(w => (double) w.ElapsedMilliseconds).Median();

			long totalIterations = workers.Select(w => (long)w.Iterations).Aggregate(0L, (c, n) => c + n);
			long avgIterations = totalIterations / Math.Max(workers.Length, 1);
			long medianIterations = (long)workers.Select(w => (double)w.Iterations).Median();

			double totalCPU = perfCollector.Select(cpu => cpu).Aggregate(0.0, (c, n) => c + n);
			double avgCPU = totalCPU / Math.Max(perfCollector.Count(), 1);
			double medianCPU = perfCollector.Median();

			double totalThroughput = workers.Select(w => w.ThroughputPerMillisecond).Aggregate(0.0, (c, n) => c + n);
			double avgThroughput = totalThroughput / Math.Max(workers.Length, 1);
			double medianThroughput = workers.Select(w => w.ThroughputPerMillisecond).Median();

			perfCollector
			.Select(cpu => ReportUC
							.New<BenchInfoNames>(ReportTypeUC.CVS)
							.Report(name, BenchInfoNames.Name)
							.Report($"{cpu}", BenchInfoNames.CPU)
							.Report($"{workers.Length}", BenchInfoNames.Threads)
							.Report($"{avgTime}", BenchInfoNames.Time_ms)
							.Report($"{avgSpins}", BenchInfoNames.Spins)
							.Report($"{avgIterations}", BenchInfoNames.AvgIterations)
							.ToString())
			.Aggregate((c, n) => $"{c}{Environment.NewLine}{n}")
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
			.Report(name, BenchInfoNames.Name)
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

		public async Task HeavySequential(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			await GeneralBenchmark(perfCollector, info.Name, threads, i => new object(), (i,perfCounter,sync) => new BenchInstance[] {new SequentialWorker(perfCounter, ts, spin, string.Empty)});
		}

		public async Task HeavyLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			ILockUC Lock = new LockUC();
			await GeneralBenchmark(perfCollector, info.Name, threads, i => Lock, (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, Lock, ts, spin, string.Empty)});
		}

		public async Task HeavySpinLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			ILockUC Lock = new SpinLockUC();
			await GeneralBenchmark(perfCollector, info.Name, threads, i => Lock, (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, Lock, ts, spin, string.Empty)});
		}

		public async Task HeavyTicketSpinLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			ILockUC Lock = new TicketSpinLockUC();
			await GeneralBenchmark(perfCollector, info.Name, threads, i => Lock, (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, Lock, ts, spin, string.Empty)});
		}

		public async Task HeavyMonitorLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			#pragma warning disable 618
			ILockUC Lock = new MonitorLockUC();
			#pragma warning restore 618
			await GeneralBenchmark(perfCollector, info.Name, threads, i => Lock, (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, Lock, ts, spin, string.Empty)});
		}

		public async Task HeavyAsyncLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threads)
		{
			IAsyncLockUC Lock = new AsyncLockUC();
			await GeneralBenchmark(perfCollector, info.Name, threads, i => Lock, (i, perfCounter, sync) => new BenchInstance[] { new AsyncLockWorker(perfCounter, Lock, ts, spin, string.Empty),  });
		}

		public async Task NeighborAsyncLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new AsyncLockUC(), (i, perfCounter, sync) => new BenchInstance[] { new AsyncLockWorker(perfCounter, sync, ts, spin, $"A{i}"), new AsyncLockWorker(perfCounter, sync, ts, spin, $"B{i}") });
		}

		public async Task NeighborLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new LockUC(), (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, sync, ts, spin, $"A{i}"), new SimpleLockWorker(perfCounter, sync, ts, spin, $"B{i}") });
		}

		public async Task NeighborSpinLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new SpinLockUC(), (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, sync, ts, spin, $"A{i}"), new SimpleLockWorker(perfCounter, sync, ts, spin, $"B{i}") });
		}

		public async Task NeighborTicketSpinLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new TicketSpinLockUC(), (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, sync, ts, spin, $"A{i}"), new SimpleLockWorker(perfCounter, sync, ts, spin, $"B{i}") });
		}

		public async Task NeighborMonitorLockUC(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			#pragma warning disable 618
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new MonitorLockUC(), (i, perfCounter, sync) => new BenchInstance[] { new SimpleLockWorker(perfCounter, sync, ts, spin, $"A{i}"), new SimpleLockWorker(perfCounter, sync, ts, spin, $"B{i}") });
			#pragma warning restore 618
		}

		public async Task HeavyConcurrentQueue(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => string.Empty, (i, perfCounter, sync) => new BenchInstance[] { new ConcurrentQueueWriterWorker(queue, perfCounter, ts, spin, $"W{i}"), new ConcurrentQueueWriterWorker(queue, perfCounter, ts, spin, $"R{i}") });
		}
		
		public async Task ManualResetEvent(BenchInfo info, IPerfCounterCollectorUC perfCollector, TimeSpan ts, int spin, int threadPairs)
		{
			await GeneralBenchmark(perfCollector, info.Name, threadPairs, i => new ManualResetEvent(false), (i, perfCounter, sync) => new BenchInstance[] { new ManualResetEventSetterWorker(perfCounter, sync, ts, spin, $"A{i}"), new ManualResetEventReSetterWorker(perfCounter, sync, ts, spin, $"B{i}") });
		}

	}
}
