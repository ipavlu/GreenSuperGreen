using System.Threading.Tasks;
using GreenSuperGreen.Diagnostics;

namespace GreenSuperGreen.Benchmarking.Launcher
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			DiagnosticsUC.RegisterProcessInstanceNameFactory(new ProcessInstanceNameFactoryUC());
			DiagnosticsUC.RegisterPerfCounterFactory(new ProcessorTimePerfCounterFactoryUC());
			await BenchmarkingInitPoint.MainAsync(args);
		}
	}
}
