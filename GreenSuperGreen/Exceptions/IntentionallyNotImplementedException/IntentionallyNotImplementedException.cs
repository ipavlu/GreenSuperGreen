using System;
using System.Runtime.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Exceptions
{
	public class IntentionallyNotImplementedException : NotImplementedException
	{
		public IntentionallyNotImplementedException() { }

		public IntentionallyNotImplementedException(string message) : base(message) { }

		public IntentionallyNotImplementedException(string message, Exception innerException) : base(message, innerException) { }

		protected IntentionallyNotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
