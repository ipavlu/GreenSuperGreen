using System;
using System.Diagnostics;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Diagnostics
{
	public static class DiagnosticsUC
	{
		public static IPerfCounterUC PerfCounter(PerfCounterTypeUC perfCounterType, string processName = null, int? pid = null)
		{
			if (perfCounterType == PerfCounterTypeUC.ProcessorTime) return new ProcessorTimePerfCounterUC(processName,pid);
			return null;
		}

		public static IPerfCounterCollectorUC PerfCounterCollector(PerfCounterTypeUC perfCounterType, TimeSpan period, string processName = null, int? pid = null)
		{
			if (perfCounterType == PerfCounterTypeUC.ProcessorTime) return new PerfCounterCollectorUC(perfCounterType, period, processName, pid);
			return null;
		}

		public static async Task RepeatedUpdate(this IPerfCounterUC perfCounter, Task done, int milliseconds, Action<int, IPerfCounterUC> action)
		{
			if (perfCounter == null || done == null || action == null) return;
			int counter = 0;

			do
			{
				await Task.Delay(milliseconds);
				action(counter++, perfCounter);
			} while (!done.IsCompleted);
		}

		public static string CurrentProcessInstanceName() => ProcessInstanceName(Process.GetCurrentProcess().Id);

		public static string ProcessInstanceName(int pid)
		{
			string[] instances = new PerformanceCounterCategory("Process").GetInstanceNames();

			for (int i = 0; i < instances.Length; ++i)
			{
				string instance = instances[i];

				using (PerformanceCounter counter = new PerformanceCounter("Process", "ID Process", instance, true))
				{
					if ((int)counter.RawValue == pid) return instance;
				}
			}
			throw new InvalidOperationException();
		}
	}
}
