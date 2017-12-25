using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class CompletionUCGenericPostAwait : ACompletionUC<CompletionUCGenericPostAwait, bool>
		{
			private int _sequence;

			public override ICompletionUC<bool> GetAwaiter()
			{
				if (Interlocked.Increment(ref _sequence) != 1) throw new InvalidOperationException("GetAwaiter was not first");
				return base.GetAwaiter();
			}

			public override void OnCompleted(Action continuation)
			{
				base.OnCompleted(continuation);
				if (Interlocked.Increment(ref _sequence) != 2) throw new InvalidOperationException("OnCompleted was not second");
			}

			public bool Complete(bool rslt)
			{
				if (Interlocked.Increment(ref _sequence) != 3) throw new InvalidOperationException("SetCompletion was not third");
				return SetCompletion(rslt);
			}
		}

		[Test]
		public async Task TestCompletionGeneric_PostAwaitComplete()
		{
			CompletionUCGenericPostAwait cpl = new CompletionUCGenericPostAwait();
			ICompletionUC<bool> icpl = cpl;

			#pragma warning disable 4014
			Task.Delay(1000).ContinueWith(task =>
			{
				Trace.WriteLine("PostAwaitComplete");
				cpl.Complete(true);
			});
			#pragma warning restore 4014

			Trace.WriteLine("BeginAwait");
			await icpl;
			Trace.WriteLine("EndAwait");
		}
	}
}
