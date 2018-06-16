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
		private class AwaitActivatedProcessingAsync : ACompletionUC<AwaitActivatedProcessingAsync>
		{
			private int _counter;

			public override ICompletionUC GetAwaiter()
			{
				if (Interlocked.Increment(ref _counter) == 1)
				{
					Task.Run((Action)Processing);
				}
				return base.GetAwaiter();
			}

			private void Processing()
			{
				Trace.WriteLine("Processsing initiated");
				SetCompletion();
				Trace.WriteLine("Processsing completed");
			}
		}

		[Test]
		public async Task ExampleAwaitActivatedProcessingAsyncTest()
		{
			AwaitActivatedProcessingAsync cpl = new AwaitActivatedProcessingAsync();
			ICompletionUC icpl = cpl;

			Trace.WriteLine("await");
			await icpl;
			Trace.WriteLine("await completed");
		}
	}
}
