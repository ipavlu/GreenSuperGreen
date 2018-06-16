using System;
using System.Threading;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public interface ISequencerExceptionRegister
	{
		CancellationToken Token { get; }
		ISequencerExceptionRegister RegisterException(Exception exception);
		ISequencerExceptionRegister TryReThrowException();
		Exception TryGetException();
	}
}