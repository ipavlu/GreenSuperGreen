using System;
using Concurrent.FastReflection.NetCore;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IBenchmarkFactory
	{
		Type BenchmarkType { get; }
		IBenchmark GetBenchmark(IBenchmarkConfiguration benchmarkConfiguration);
	}

	public class BenchmarkFactory<TBenchmark> : IBenchmarkFactory where TBenchmark : class, IBenchmark
	{
		public Type BenchmarkType { get; } = typeof(TBenchmark);

		private CtorInvoker<TBenchmark> ConstructorInvoker { get; }

		public BenchmarkFactory()
		{
			var ctor = BenchmarkType.DelegateForCtor<TBenchmark>(typeof(IBenchmarkConfiguration));
			ConstructorInvoker = ctor;
		}

		public IBenchmark GetBenchmark(IBenchmarkConfiguration benchmarkConfiguration)
		{
			var aa = ConstructorInvoker?.Invoke(new object[]{ benchmarkConfiguration});
			var bb = (IBenchmark) aa;
			return bb;
		} 
	}
}