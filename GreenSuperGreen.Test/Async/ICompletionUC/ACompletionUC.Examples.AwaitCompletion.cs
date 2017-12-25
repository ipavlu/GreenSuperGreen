using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming


namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class AwaitCompletion : ACompletionUC<AwaitCompletion>
		{
			public bool Completion() => SetCompletion();
		}

		[Test]
		public async Task ExampleAwaitCompletionTest()
		{
			AwaitCompletion cpl = new AwaitCompletion();
			ICompletionUC icpl = cpl;

			#pragma warning disable 4014
			Task.Delay(5).ContinueWith(t =>
			#pragma warning restore 4014
			{
				Assert.IsTrue(cpl.Completion()); //setting completion when some operation is completed
			});

			await icpl;

		
		}
	}
}
