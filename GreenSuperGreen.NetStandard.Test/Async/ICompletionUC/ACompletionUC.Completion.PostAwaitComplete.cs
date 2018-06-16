using System;
using System.Diagnostics;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class CompletionUCPostAwait : ACompletionUC<CompletionUCPostAwait> 
		{
			private int _sequence;

			public override ICompletionUC GetAwaiter()
			{
				if (Interlocked.Increment(ref _sequence) != 1) throw new InvalidOperationException("GetAwaiter was not first");
				return base.GetAwaiter();
			}

			public override void OnCompleted(Action continuation)
			{
				base.OnCompleted(continuation);
				if (Interlocked.Increment(ref _sequence) != 2) throw new InvalidOperationException("OnCompleted was not second");
			}

			[SecurityCritical]
			public override void UnsafeOnCompleted(Action continuation)
			{
				base.UnsafeOnCompleted(continuation);
				if (Interlocked.Increment(ref _sequence) != 2) throw new InvalidOperationException("UnsafeOnCompleted was not second");
			}

			public void Complete()
			{
				if (Interlocked.Increment(ref _sequence) != 3) throw new InvalidOperationException("SetCompletion was not third");
				SetCompletion();
			}
		}

		[Test]
		public async Task TestCompletionUC_PostAwaitComplete()
		{
			CompletionUCPostAwait cpl = new CompletionUCPostAwait();
			ICompletionUC icpl = cpl;

			#pragma warning disable 4014
			Task.Delay(1000).ContinueWith(task =>
			{
				Trace.WriteLine("PostAwaitComplete");
				cpl.Complete();
			});
			#pragma warning restore 4014

			Trace.WriteLine("BeginAwait");
			await icpl;
			Trace.WriteLine("EndAwait");
		}
	}
}
