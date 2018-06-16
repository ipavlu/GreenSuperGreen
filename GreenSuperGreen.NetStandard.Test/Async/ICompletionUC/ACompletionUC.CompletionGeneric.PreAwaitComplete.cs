using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class CompletionUCGenericPreAwaitComplete : ACompletionUC<CompletionUCGenericPreAwaitComplete, bool>
		{
			private int _sequence;

			public override ICompletionUC<bool> GetAwaiter()
			{
				if (Interlocked.Increment(ref _sequence) != 2) throw new InvalidOperationException("GetAwaiter was not second");
				return base.GetAwaiter();
			}

			public override void OnCompleted(Action continuation)
			{
				base.OnCompleted(continuation);
				if (Interlocked.Increment(ref _sequence) != 3) throw new InvalidOperationException("OnCompleted was not third");
			}

			public  bool Complete(bool rslt)
			{
				if (Interlocked.Increment(ref _sequence) != 1) throw new InvalidOperationException("SetCompletion was not first");
				return SetCompletion(rslt);
			}
		}

		[Test]
		public async Task TestCompletionGeneric_PreAwaitComplete()
		{
			var cpl = new CompletionUCGenericPreAwaitComplete();
			ICompletionUC<bool> icpl = cpl;

			Trace.WriteLine("PreAwaitComplete");
			cpl.Complete(true);

			Trace.WriteLine("BeginAwait");
			await icpl;
			Trace.WriteLine("EndAwait");
		}
	}
}
