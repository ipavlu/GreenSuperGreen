// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Diagnostics
{
	public interface IPerfCounterFactoryUC
	{
		PerfCounterTypeUC PerfCounterType { get; }
		IPerfCounterUC NewPerfCounterUC(string processName = null, int? pid = null);
	}
}
