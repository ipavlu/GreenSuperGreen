using System;
using System.Diagnostics;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Diagnostics
{
	internal class ProcessInstanceNameFactoryUC : IProcessInstanceNameFactoryUC
	{
		public IProcessInstanceNameUC CreateProcessInstanceNameUC()
		=> new ProcessInstanceNameUC()
		;
	}

	internal class ProcessInstanceNameUC : IProcessInstanceNameUC
	{
		public string GetProcessInstanceName(int? processId = null)
		{
			processId = processId ?? Process.GetCurrentProcess().Id;

			string[] instances = new PerformanceCounterCategory("Process").GetInstanceNames();

			for (int i = 0; i < instances.Length; ++i)
			{
				using (PerformanceCounter counter = new PerformanceCounter("Process", "ID Process", instances[i], true))
				{
					if ((int)counter.RawValue == processId) return instances[i];
				}
			}
			throw new InvalidOperationException($"{nameof(ProcessInstanceNameUC)} has failed to detect process instance name");
		}
	}
}
