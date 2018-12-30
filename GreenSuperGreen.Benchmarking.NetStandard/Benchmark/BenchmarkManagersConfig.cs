using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace GreenSuperGreen.Benchmarking
{
	public enum Collector 
	{
		Collect,
		Skip
	}

	public static class BenchmarkManagersConfig
	{
		public static ImmutableDictionary<string, IBenchmarkManager> Tests { get; }

		public static IBenchmarkManager GetBenchmarkManager(this string keyName) =>
			Tests.TryGetValue(keyName ?? "wrong key", out IBenchmarkManager benchmarkManager) && benchmarkManager != null
				? benchmarkManager
				: null
		;

		public static ImmutableArray<IBenchmarkManager> BenchmarkManagers { get; }

		static BenchmarkManagersConfig()
		{
			BenchmarkManagers = BenchmarkManagersBuilder();
			Tests = BenchmarkManagers.ToImmutableDictionary(x => x.KeyName);
		}


		public static ImmutableArray<IBenchmarkManager> BenchmarkManagersBuilder() =>
		new List<IBenchmarkManager>()
		.AddBenchInfo<HeavySequential>("0", Collector.Skip)
		.AddBenchInfo<DataCollectorBenchmark>("collect", Collector.Skip)

		.AddBenchInfo<HeavyLockUC>("1", Collector.Collect)
		.AddBenchInfo<NeighborLockUC>("q", Collector.Collect)
		.AddBenchInfo<HeavyAsyncLockUC>("a", Collector.Collect)
		.AddBenchInfo<NeighborAsyncLockUC>("z", Collector.Collect)
		
		.AddBenchInfo<HeavySpinLockUC>("2", Collector.Collect)
		.AddBenchInfo<NeighborSpinLockUC>("w", Collector.Collect)
		.AddBenchInfo<HeavyAsyncSpinLockUC>("s", Collector.Skip)
		.AddBenchInfo<NeighborAsyncSpinLockUC>("x", Collector.Skip)

		.AddBenchInfo<HeavyTicketSpinLockUC>("3", Collector.Collect)
		.AddBenchInfo<NeighborTicketSpinLockUC>("e", Collector.Collect)
		.AddBenchInfo<HeavyAsyncTicketSpinLockUC>("d", Collector.Skip)
		.AddBenchInfo<NeighborAsyncTicketSpinLockUC>("c", Collector.Skip)

		.AddBenchInfo<HeavySemaphoreSlimLockUC>("4", Collector.Collect)
		.AddBenchInfo<NeighborSemaphoreSlimLockUC>("r", Collector.Collect)
		.AddBenchInfo<HeavyAsyncSemaphoreSlimLockUC>("f", Collector.Collect)
		.AddBenchInfo<NeighborAsyncSemaphoreSlimLockUC>("v", Collector.Collect)

		.AddBenchInfo<HeavyMonitorLockUC>("t", Collector.Collect)
		.AddBenchInfo<NeighborMonitorLockUC>("g", Collector.Collect)

		.AddBenchInfo<HeavyMutexLockUC>("y", Collector.Collect)
		.AddBenchInfo<NeighborMutexLockUC>("h", Collector.Collect)

		.AddBenchInfo<HeavySemaphoreLockUC>("u", Collector.Collect)
		.AddBenchInfo<NeighborSemaphoreLockUC>("j", Collector.Collect)

		//.AddBenchInfo<ManualResetEvent1>("b", Collector.Skip)
		//.AddBenchInfo<ManualResetEvent2>("n", Collector.Skip)
		//.AddBenchInfo<ManualResetEvent4>("m", Collector.Skip)
		//.AddBenchInfo<ManualResetEventHalf>("o", Collector.Skip)

		//.AddBenchInfo<BalancedProdConsConcurrentQueue>("i", Collector.Skip)
		//.AddBenchInfo<BalancedProdConsHalfConcurrentQueue>("k", Collector.Skip)
		
		.ToImmutableArray()
		;


		public
		static
		List<IBenchmarkManager>
		AddBenchInfo<TBenchmark>(this	List<IBenchmarkManager> list,
										string keyName,
										Collector collectorSupport)
		where TBenchmark : class, IBenchmark
		{
			keyName = keyName.ToLower();
			var bm = new BenchmarkManager<TBenchmark>(list.Count, keyName, collectorSupport);
			list.Add(bm);
			return list;
		}
	}
}