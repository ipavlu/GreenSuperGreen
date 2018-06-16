using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class AwaitActivatedProcessingSync : ACompletionUC<AwaitActivatedProcessingSync>
		{
			private int _counter;

			public override ICompletionUC GetAwaiter()
			{
				if (Interlocked.Increment(ref _counter) == 1)
				{
					Processing();
				}
				return base.GetAwaiter();
			}

			private void Processing()
			{
				Trace.WriteLine("Processsing Initiated");
				SetCompletion();
				Trace.WriteLine("Processsing Completed");
			}
		}

		[Test]
		public async Task ExampleAwaitActivatedProcessingSyncTest()
		{
			AwaitActivatedProcessingSync cpl = new AwaitActivatedProcessingSync();
			ICompletionUC icpl = cpl;

			Trace.WriteLine("await");
			await icpl;
			Trace.WriteLine("await completed");
		}
	}
}
