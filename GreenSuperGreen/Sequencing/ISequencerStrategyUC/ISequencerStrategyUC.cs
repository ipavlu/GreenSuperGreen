using System;
using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Sequencing
{
	public interface ISequencerStrategyUC
	{
		ICompletionUC ProductionPoint(	ISequencerTaskRegister taskRegister,
										ISequencerExceptionRegister exceptionRegister,
										SeqPointTypeUC seqPointTypeUC,
										object arg = null,
										Action<object> injectContinuation = null);
		ITestPointUC TestPoint(	ISequencerTaskRegister taskRegister,
								ISequencerExceptionRegister exceptionRegister);
	}
}