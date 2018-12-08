// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GreenSuperGreen.UnifiedConcurrency;

namespace GreenSuperGreen.Diagnostics
{
	public class PerfCounterCollectorUC : IPerfCounterCollectorUC
	{
		private ILockUC Lock { get; } = new SpinLockUC();
		private Stopwatch StopWatch { get; set; } = new Stopwatch();
		private IPerfCounterUC PerfCounter { get; set; }
		private Queue<double> Queue { get; set; } = new Queue<double>();
		private HashSet<object> Unique { get; set; } = new HashSet<object>();
		private int? MinUniques { get; set; }
		private int ReadCounter { get; set; }

		private TimeSpan Period { get; }
		private TimeSpan Next { get; set; }

		public PerfCounterCollectorUC(PerfCounterTypeUC perfCounterType, TimeSpan period, string processName = null, int? pid = null)
		{
			Next = Period = period;
			PerfCounter = perfCounterType.CreatePerfCounter(processName, pid);
		}

		public bool? TryStart(int? minUniques = null)
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				MinUniques = minUniques;
				if (entry.EntryTypeUC == EntryTypeUC.None) return null;
				if (StopWatch == null) return false; //disposed
				if (StopWatch.IsRunning) return true;
				Next = Period;
				StopWatch.Reset();
				StopWatch.Start();
				return true;
			}
		}

		public bool? TryStop()
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				if (entry.EntryTypeUC == EntryTypeUC.None) return null;
				if (StopWatch == null) return false; //disposed
				if (!StopWatch.IsRunning) return true;
				StopWatch.Stop();
				StopWatch.Reset();
				return true;
			}
		}

		public bool? TryClear()
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				if (entry.EntryTypeUC == EntryTypeUC.None) return null;
				if (Queue == null) return false; //disposed
				if (StopWatch?.IsRunning == true) return false;
				Queue.Clear();
				Unique.Clear();
				ReadCounter = 0;
				return true;
			}
		}

		public TimeSpan? TryRead(object unique = null)
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				if (entry.EntryTypeUC == EntryTypeUC.None) return null;
				if (StopWatch == null) return null;//disposed
				if (!StopWatch.IsRunning) return null;
				Unique.Add(unique);
				if (Unique.Count < MinUniques) return null;

				TimeSpan now = StopWatch.Elapsed;
				if (Next > now) return Next - now;//not yet
				Next += Period;
				if (ReadCounter++ > 0 || MinUniques == null)
				{
					Queue.Enqueue(PerfCounter.Performance);
				}
				else
				{
					// ReSharper disable once UnusedVariable
					var aa = PerfCounter.Performance;
				}
				return Next - now;
			}
		}

		public void Dispose()
		{
			using (Lock.Enter())
			{
				StopWatch?.Stop();
				PerfCounter?.Dispose();
				Queue?.Clear();
				Unique?.Clear();

				StopWatch = null;
				PerfCounter = null;
				Queue = null;
				Unique = null;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<double> GetEnumerator()
		{
			using (Lock.Enter())
			{
				return (Queue?.ToArray().AsEnumerable() ?? Enumerable.Empty<double>()).GetEnumerator();
			}
		}
	}
}
