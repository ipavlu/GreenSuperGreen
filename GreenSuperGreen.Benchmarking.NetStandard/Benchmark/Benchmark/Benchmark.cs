using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenSuperGreen.Benchmarking
{
	public class Benchmark<TSyncPrimitive> : BenchmarkConfiguration, IBenchmark<TSyncPrimitive>
		where TSyncPrimitive : class
	{
		private IDictionary<IThreadGroupIndex, TSyncPrimitive> SyncPrimitiveCache { get; } = new Dictionary<IThreadGroupIndex, TSyncPrimitive>();

		protected virtual TSyncPrimitive SharedSyncPrimitiveFactory => default(TSyncPrimitive);
		protected virtual TSyncPrimitive SyncPrimitiveFactory(IThreadGroupIndex threadGroupIndex) => default(TSyncPrimitive);

		protected virtual BenchmarkWorker[] BenchInstanceGenerator(TSyncPrimitive syncprimitive, IThreadGroupIndex threadGroupIndex)
		=> Array.Empty<BenchmarkWorker>()
		;

		public TSyncPrimitive CreateOrGetStoredSyncPrimitive(IThreadGroupIndex threadGroupIndex)
		{
			if (threadGroupIndex == null) return SharedSyncPrimitiveFactory;

			if (SyncPrimitiveCache.TryGetValue(threadGroupIndex, out TSyncPrimitive result) && result != default(TSyncPrimitive))
			{
				return result;
			}

			if ((result = SyncPrimitiveFactory(threadGroupIndex)) != default(TSyncPrimitive))
			{
				return SyncPrimitiveCache[threadGroupIndex] = result;
			}

			return SharedSyncPrimitiveFactory;
		}

		public BenchmarkWorker[] CreateBenchInstance(IThreadGroupIndex threadGroupIndex)
		=> BenchInstanceGenerator(CreateOrGetStoredSyncPrimitive(threadGroupIndex), threadGroupIndex)
		;

		protected virtual string NamePrefix => string.Empty;
		protected virtual string NameBase => GetType().Name;
		protected virtual string NamePostfix => string.Empty;

		public string Name => $"{NamePostfix}{NameBase}{NamePostfix}";

		protected virtual int RawThreadGroups { get; } = 0;
		public int ThreadGroups => Math.Max(RawThreadGroups, 1);
		
		protected Benchmark(IBenchmarkConfiguration test) :  base(test) { }

		public async Task PerfCollectorTryClearAsync(int delay = 100)
		{
			while (PerfCollector.TryClear() != true) { await Task.Delay(100); }
		}

		public async Task PerfCollectorTryStartAsync(int delay = 100)
		{
			while (PerfCollector.TryStart() != true) { await Task.Delay(100); }
		}

		public async Task PerfCollectorTryStopAsync(int delay = 100)
		{
			while (PerfCollector.TryStop() != true) { await Task.Delay(100); }
		}

		public virtual Task ExecuteBenchmarkAsync() => BenchmarkProcessor.Execute(this);
	}
}