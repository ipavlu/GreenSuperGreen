using System;
using Consurrent.FastReflection.NetCore;

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
			var type = BenchmarkType;
			var ctor = BenchmarkType.DelegateForCtor<TBenchmark>(typeof(IBenchmarkConfiguration));
			ConstructorInvoker = ctor;

		}

		public IBenchmark GetBenchmark(IBenchmarkConfiguration benchmarkConfiguration)
		{
			var aa = ConstructorInvoker?.Invoke(new object[]{ benchmarkConfiguration as object});
			var bb = aa as IBenchmark;
			return bb;
		} 
	}
}