using System;
using System.Collections.Generic;
using System.Threading;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public class SequencerExceptionRegister : ISequencerExceptionRegister
	{
		private Queue<Exception> Exceptions { get; } = new Queue<Exception>();
		private ILockUC Lock { get; } = new SpinLockUC();
		private AggregateException AggregateException { get; set; }
		private CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

		public CancellationToken Token => TokenSource.Token;

		public ISequencerExceptionRegister RegisterException(Exception exception)
		{
			if (exception == null) return this;

			using (Lock.Enter())
			{
				Exceptions.Enqueue(exception);
				AggregateException = new AggregateException(Exceptions);
			}
			TokenSource.Cancel(false);
			return this;
		}
		public Exception TryGetException()
		{
			using (Lock.Enter())
			{
				return TryGetExceptionLocked();
			}
		}
		public Exception TryGetExceptionLocked()
		{
			if (AggregateException == null) return null;
			Exceptions.Enqueue(new Exception("Exception detected, throwned elsewhere!"));
			return AggregateException = new AggregateException(Exceptions);
		}

		public ISequencerExceptionRegister TryReThrowException()
		{
			using (Lock.Enter())
			{
				if (TryGetExceptionLocked() == null) return this;
				throw AggregateException;
			}
		}
	}
}