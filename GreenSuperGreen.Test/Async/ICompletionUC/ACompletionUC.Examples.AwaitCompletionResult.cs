using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming


namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class AwaitCompletionResult : ACompletionUC<AwaitCompletionResult, bool>
		{
			public bool Completion(bool rslt) => SetCompletion(rslt);
		}

		[Test]
		public async Task ExampleAwaitCompletionResultTest()
		{
			AwaitCompletionResult cpl = new AwaitCompletionResult();
			ICompletionUC<bool> icpl = cpl;

#pragma warning disable 4014
			Task.Delay(5).ContinueWith(t =>
#pragma warning restore 4014
			{
				Assert.IsTrue(cpl.Completion(true));//setting completion with result when some operation is completed
			});

			Assert.IsTrue(await icpl);
		}
	}
}
