using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class CompletionUCPreAwaitComplete : ACompletionUC<CompletionUCPreAwaitComplete>
		{
			private int _sequence;

			public override ICompletionUC GetAwaiter()
			{
				if (Interlocked.Increment(ref _sequence) != 2) throw new InvalidOperationException("GetAwaiter was not second");
				return base.GetAwaiter();
			}

			public override void OnCompleted(Action continuation)
			{
				base.OnCompleted(continuation);
				if (Interlocked.Increment(ref _sequence) != 3) throw new InvalidOperationException("OnCompleted was not third");
			}

			public void Complete()
			{
				if (Interlocked.Increment(ref _sequence) != 1) throw new InvalidOperationException("SetCompletion was not first");
				SetCompletion();
			}
		}

		[Test]
		public async Task TestCompletion_PreAwaitComplete()
		{
			var cpl = new CompletionUCPreAwaitComplete();
			ICompletionUC icpl = cpl;

			Trace.WriteLine("PreAwaitComplete");
			cpl.Complete();

			Trace.WriteLine("BeginAwait");
			await icpl;
			Trace.WriteLine("EndAwait");
		}
	}
}
