using System;
using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public interface ITestPointUC : ICompletionUC<IProductionPointUC>
	{
		void Complete(IProductionPointUC productionPoint = null);
		void Fail(Exception exception = null);
	}
}
