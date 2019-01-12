using System.Threading;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Benchmarking
{
	public class ManualResetEventSetterWorker : BenchmarkWorker
	{
		private ManualResetEvent MRE { get; }
		public ManualResetEventSetterWorker(ManualResetEvent syncPrimitive,
											IBenchmarkConfiguration benchmarkConfiguration,
											string resourceName,
											string pair)
			: base(benchmarkConfiguration, resourceName, pair)
		{ MRE = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			WastingTime();
			MRE.Set();
			await Task.CompletedTask;
		}
	}
}