using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class ConditionalSequencerUC
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public static void Throw(	ISequencerUC sequencer,
									Func<bool> condition,
									Exception innerException = null,
									string message = null,
									int skipStackFrames = 3,
									[CallerMemberName] string calllerName = "",
									[CallerFilePath] string callerFileName = "",
									[CallerLineNumber] int callerLineNumber = 0)
		{
			sequencer?.Throw(condition, innerException, message, skipStackFrames, calllerName, callerFileName, callerLineNumber);
		}


		[MethodImpl(MethodImplOptions.NoInlining)]
		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public static void Throw(	ISequencerUC sequencer,
									Exception innerException = null,
									string message = null,
									int skipStackFrames = 4,
									[CallerMemberName] string calllerName = "",
									[CallerFilePath] string callerFileName = "",
									[CallerLineNumber] int callerLineNumber = 0)
		{
			sequencer?.Throw(innerException, message, skipStackFrames, calllerName, callerFileName, callerLineNumber);
		}
	}
}
