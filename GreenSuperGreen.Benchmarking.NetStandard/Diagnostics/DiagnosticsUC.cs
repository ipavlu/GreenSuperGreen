using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Diagnostics
{
	public static partial class DiagnosticsUC
	{
		private static ILockUC Lock { get; } = new SpinLockUC();
		
		private static IDictionary<PerfCounterTypeUC, IPerfCounterFactoryUC> Factories { get; }
		= new Dictionary<PerfCounterTypeUC, IPerfCounterFactoryUC>()
		;

		private static readonly int ProcessID = Process.GetCurrentProcess().Id;
		private static string ProcessName;

		public
		static
		void
		RegisterProcessInstanceNameFactory(IProcessInstanceNameFactoryUC processInstanceNameFactory) =>
		processInstanceNameFactory
		?.CreateProcessInstanceNameUC()
		?.GetProcessInstanceName(ProcessID)
		?.AssignRef(ref ProcessName)
		;

		public static string GetProcessName()
		{
			using (Lock.Enter())
			{
				return ProcessName;
			}
		}

		public static void RegisterPerfCounterFactory(IPerfCounterFactoryUC factory)
		{
			if (factory == null) throw new ArgumentNullException(nameof(factory));
			using (Lock.Enter())
			{
				Factories[factory.PerfCounterType] = factory;
			}
		}

		public static IPerfCounterUC CreatePerfCounter(this PerfCounterTypeUC perfCounterType, string processName = null, int? pid = null)
		{
			IPerfCounterFactoryUC factory;
			using (Lock.Enter())
			{
				if (!Factories.TryGetValue(perfCounterType, out factory) || factory == null)
				{
					throw new ArgumentException($"{nameof(PerfCounterTypeUC)}.{perfCounterType} not registered!");
				}
			}

			return factory.NewPerfCounterUC(processName, pid);
		}
	}

	public static partial class DiagnosticsUC
	{
		public static IPerfCounterCollectorUC PerfCounterCollector(this PerfCounterTypeUC perfCounterType, TimeSpan period, string processName = null, int? pid = null)
		=> new PerfCounterCollectorUC(perfCounterType, period, processName, pid);

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
	}
}
