using System;
using GreenSuperGreen.Async;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public
		static
		ICompletionUC
		PointAsync<TEnum>(this	ISequencerUC sequencer,
								SeqPointTypeUC seqPointTypeUC,
								TEnum registration,
								object arg = null,
								Action<object> injectContinuation = null)
		where TEnum : struct
		{
			return
			sequencer
			.PointAsync(seqPointTypeUC,
						registration,
						() => true,
						arg,
						injectContinuation)
			;
		}

		public
		static
		ICompletionUC
		PointAsync<TEnum>(this	ISequencerUC sequencer,
								SeqPointTypeUC seqPointTypeUC,
								TEnum registration,
								Func<bool> condition,
								object arg = null,
								Action<object> injectContinuation = null)
		where TEnum : struct
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null || condition == null || !condition()) return CompletedAwaiter;

			ISequencerExceptionRegister exceptionRegister = register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			ISequencerPointUC<TEnum> seqPoint = register.TryGet(registration);
			return
			seqPoint?.ProductionPoint(taskRegister,exceptionRegister, seqPointTypeUC, arg, injectContinuation)
			?? CompletedAwaiter
			;
		}
	}
}