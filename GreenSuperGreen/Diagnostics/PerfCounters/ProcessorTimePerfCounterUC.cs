using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Diagnostics
{
	public class ProcessorTimePerfCounterUC : IPerfCounterUC, IDisposable, IEnumerable<double>
	{
		private CounterSample StartSample { get; set; }
		private PerformanceCounter Counter { get; set; }
		private Timer Timer { get; set; }
		private Queue<double> Collected { get; set; }
		private ILockUC Lock { get; } = new SpinLockUC();

		public PerfCounterTypeUC PerfCounterType => PerfCounterTypeUC.ProcessorTime;

		public ProcessorTimePerfCounterUC(string processName = null, int? pid = null)
		{
			if (processName == null && pid == null)
			{
				Counter = new PerformanceCounter("Process", "% Processor Time", DiagnosticsUC.CurrentProcessInstanceName(), true);
				ResetCounter();
			}
			else if (processName != null)
			{
				Counter = new PerformanceCounter("Process", "% Processor Time", processName, true);
				ResetCounter();
			}
			else
			{
				Counter = new PerformanceCounter("Process", "% Processor Time", DiagnosticsUC.ProcessInstanceName(pid.Value), true);
				ResetCounter();
			}
		}

		public void ResetCounter() => StartSample = Counter?.NextSample() ?? CounterSample.Empty;

		public double Performance => ComputePerformance();

		private double ComputePerformance()
		{
			if (Counter == null) return default(double);
			CounterSample currentSample = Counter.NextSample();

			double perfCounterDifference = currentSample.RawValue - StartSample.RawValue;
			double timeDifference = currentSample.TimeStamp100nSec - StartSample.TimeStamp100nSec;

			StartSample = currentSample;

			return (perfCounterDifference / timeDifference) * 100.0 / Environment.ProcessorCount;
		}

		public void Dispose()
		{
			using (Lock.Enter())
			{
				if (Counter == null) return;
				StopCollectingLocked();
				Counter.Dispose();
				Counter = null;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<double> GetEnumerator()
		{
			using (Lock.Enter())
			{
				return (Collected?.ToArray() ?? Array.Empty<double>()).AsEnumerable().GetEnumerator();
			}
		}

		public IPerfCounterUC StartCollecting(int ms)
		{
			using (Lock.Enter())
			{
				if (Counter == null) return this;
				if (Timer != null) throw new InvalidOperationException(@"Already running!");
				Collected = new Queue<double>();
				Timer = new Timer(CallBack, this, 0, ms < 1 ? 1 : ms);
			}
			return this;
		}

		private void CallBack(object obj)
		{
			using (Lock.Enter())
			{
				if (Counter == null) return;
				if (Timer == null) return;
				Collected.Enqueue(Performance);
			}
		}

		private IPerfCounterUC StopCollectingLocked()
		{
			Timer?.Dispose();
			Timer = null;
			return this;
		}

		public IPerfCounterUC StopCollecting()
		{
			using (Lock.Enter()) return StopCollectingLocked();
		}
	}
}
