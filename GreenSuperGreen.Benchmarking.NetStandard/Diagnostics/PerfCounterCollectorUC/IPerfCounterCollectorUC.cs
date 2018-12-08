// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;

namespace GreenSuperGreen.Diagnostics
{
	public interface IPerfCounterCollectorUC : IDisposable, IEnumerable<double>
	{
		bool? TryStart(int? minUniques = null);
		bool? TryStop();
		bool? TryClear();
		TimeSpan? TryRead(object unique = null);
	}
}
