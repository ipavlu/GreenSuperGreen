using System.Threading;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Benchmarking
{
	public class ManualResetEventReSetterWorker : BenchmarkWorker
	{
		private ManualResetEvent MRE { get; }
		public ManualResetEventReSetterWorker(	ManualResetEvent syncPrimitive,
												IBenchmarkConfiguration benchmarkConfiguration,
												string resourceName,
												string pair)
			: base(benchmarkConfiguration, resourceName, pair)
		{ MRE = syncPrimitive; }

		protected override async Task BenchmarkingTarget()
		{
			MRE.Reset();
			await Task.CompletedTask;
		}
	}
}