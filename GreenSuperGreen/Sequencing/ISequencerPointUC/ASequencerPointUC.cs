using System;
using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Sequencing
{
	public class SequencerPointUC<TEnum> : ASequencerPointUC<TEnum> where TEnum : struct
	{
		public SequencerPointUC(TEnum registration, ISequencerStrategyUC sequencerStrategy)
			: base(registration, sequencerStrategy)
		{
		}
	}

	public
	class
	ASequencerPointUC<TEnum>
	:	ISequencerPointUC<TEnum>,
		ISequencerPointUC
	where TEnum : struct
	{
		public TEnum Registration { get; }
		private ISequencerStrategyUC SequencerStrategy { get; }

		protected ASequencerPointUC(TEnum registration, ISequencerStrategyUC sequencerStrategy)
		{
			Registration = registration;
			SequencerStrategy = sequencerStrategy;
		}

		public
		virtual
		ICompletionUC
		ProductionPoint(ISequencerTaskRegister taskRegister,
						ISequencerExceptionRegister exceptionRegister,
						SeqPointTypeUC seqPointTypeUC,
						object arg = null,
						Action<object> injectContinuation = null)
		=> SequencerStrategy.ProductionPoint(taskRegister, exceptionRegister,seqPointTypeUC, arg, injectContinuation)
		;

		public
		virtual
		ITestPointUC
		TestPoint(	ISequencerTaskRegister taskRegister,
					ISequencerExceptionRegister exceptionRegister)
		=> SequencerStrategy.TestPoint(taskRegister,exceptionRegister)
		;
	}
}