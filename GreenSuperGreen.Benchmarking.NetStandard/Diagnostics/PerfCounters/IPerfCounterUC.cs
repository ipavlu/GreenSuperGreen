// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;


namespace GreenSuperGreen.Diagnostics
{
	public interface IPerfCounterUC : IDisposable, IEnumerable<double>
	{
		PerfCounterTypeUC PerfCounterType { get; }
		double Performance { get; }
		IPerfCounterUC StartCollecting(int ms);
		IPerfCounterUC StopCollecting();
	}
}
