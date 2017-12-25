using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Exceptions
{
	/// <summary>
	/// Allows to create Exception with <see cref="StackTrace"/>
	/// skipping requested number of <see cref="StackFrame"/>'s
	/// </summary>
	public class StackTraceExceptionUC : Exception
	{
		public override string StackTrace { get; }

		public StackTraceExceptionUC()
		{
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public StackTraceExceptionUC(string message, int skipStackFrames = 1)
			: base(message)
		{
			skipStackFrames = skipStackFrames < 0 ? 0 : skipStackFrames;
			StackTrace st = new StackTrace(skipStackFrames, true);
			StackTrace = st.ToString();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public StackTraceExceptionUC(string message, int skipStackFrames = 1, Exception innerException = null)
			: base(message, innerException)
		{
			skipStackFrames = skipStackFrames < 0 ? 0 : skipStackFrames;
			StackTrace st = new StackTrace(skipStackFrames, true);
			StackTrace = st.ToString();
		}

		protected StackTraceExceptionUC(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
