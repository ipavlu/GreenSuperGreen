using System.IO;
using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;
using GreenSuperGreen.TextWriterReplication;

namespace GreenSuperGreen.Benchmarking
{
	/// <summary>
	/// Automatic data collecting benchmark,
	/// capable to invoke each benchmark type under wide spectrum of configurations.
	/// </summary>
	public class DataCollectorBenchmark : Benchmark<object>, IDataCollectorBenchmark
	{
		protected override string NameBase => nameof(DataCollectorBenchmark);
		public DataCollectorBenchmark(IBenchmarkConfiguration test) : base(test) { }

		public override Task ExecuteBenchmarkAsync() => CollectData(PerfCollector, TextWriter);

		public static async Task CollectData(IPerfCounterCollectorUC perfCollector, ITextWriter textWriter)
		{
			using (var chain = File.AppendText("UnifiedConcurrencyReport.txt").InstallInto(textWriter, DisposeAncestor.Yes))
			{
				await DataCollectorProcessor.Execute(perfCollector, chain);
				await chain.TextWriterOfT.FlushAsync();
			}
		}
	}
}
