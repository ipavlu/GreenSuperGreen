using System;
using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public interface IProductionPointUC : ICompletionUC
	{
		SeqPointTypeUC SeqPointType { get; }
		bool Completed { get; }
		object ProductionArg { get; }
		void Complete(object testArg = null, SeqContinuationUC context = SeqContinuationUC.OnCapturedContext);
		void Fail(Exception exception = null);
	}
}
