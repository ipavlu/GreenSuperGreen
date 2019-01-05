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
						Constants.Boolean.FuncReturnTrue,
						arg,
						injectContinuation)
			;
		}

		/// <summary> Avoiding boxing of <see cref="ValueType"/> argument in case Sequencer is null </summary>
		public
		static
		ICompletionUC
		PointAsyncArg<TEnum,TArg>(this	ISequencerUC sequencer,
										SeqPointTypeUC seqPointTypeUC,
										TEnum registration,
										TArg arg,
										Action<object> injectContinuation = null)
		where TEnum : struct
		{
			return
			sequencer
			.PointAsyncArg(	seqPointTypeUC,
							registration,
							Constants.Boolean.FuncReturnTrue,
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
			if (!(sequencer is SequencerRegisterUC register) || condition == null || !condition()) return CompletedAwaiter;

			ISequencerExceptionRegister exceptionRegister = register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			ISequencerPointUC<TEnum> seqPoint = register.TryGet(registration);
			return
			seqPoint?.ProductionPoint(taskRegister,exceptionRegister, seqPointTypeUC, arg, injectContinuation)
			?? CompletedAwaiter
			;
		}

		/// <summary> Avoiding boxing of <see cref="ValueType"/> argument in case Sequencer is null </summary>
		public
		static
		ICompletionUC
		PointAsyncArg<TEnum,TArg>(this	ISequencerUC sequencer,
										SeqPointTypeUC seqPointTypeUC,
										TEnum registration,
										Func<bool> condition,
										TArg arg,
										Action<object> injectContinuation = null)
		where TEnum : struct
		{
			if (!(sequencer is SequencerRegisterUC register) || condition == null || !condition()) return CompletedAwaiter;

			ISequencerExceptionRegister exceptionRegister = register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			ISequencerPointUC<TEnum> seqPoint = register.TryGet(registration);
			return
			seqPoint?.ProductionPoint(taskRegister, exceptionRegister, seqPointTypeUC, arg, injectContinuation)
			?? CompletedAwaiter
			;
		}
	}
}