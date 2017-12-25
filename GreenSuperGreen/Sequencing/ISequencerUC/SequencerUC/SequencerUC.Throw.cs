using System;
using System.Runtime.CompilerServices;
using GreenSuperGreen.Exceptions;

// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ISequencerUC Throw(this ISequencerUC sequencer,
												Exception innerException = null,
												string message = null,
												int skipStackFrames = 3,
												[CallerMemberName] string calllerName = "",
												[CallerFilePath] string callerFileName = "",
												[CallerLineNumber] int callerLineNumber = 0)
		{
			return
			sequencer
			.Throw(	() => true,
					innerException,
					message,
					skipStackFrames,
					calllerName,
					callerFileName,
					callerLineNumber)
			;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ISequencerUC Throw(this	ISequencerUC sequencer,
												Func<bool> condition,
												Exception innerException = null,
												string message = null,
												int skipStackFrames = 2,
												[CallerMemberName] string calllerName = "",
												[CallerFilePath] string callerFileName = "",
												[CallerLineNumber] int callerLineNumber = 0)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null || condition == null || !condition()) return sequencer;

			string msg = string.Empty;
			msg += $"[{nameof(ISequencerUC)}.{nameof(Throw)}]";
			msg += $"[msg: {message ?? string.Empty}]";
			msg += $"[MethodName:{calllerName}]";
			msg += $"[Line:{callerLineNumber}]";
			msg += $"[FileName:{callerFileName}]";

			Exception ex = innerException != null
			? new StackTraceExceptionUC(msg, skipStackFrames, innerException)
			: new StackTraceExceptionUC(msg, skipStackFrames)
			;

			register.ExceptionRegister.RegisterException(ex);

			return sequencer;
		}
	}
}
