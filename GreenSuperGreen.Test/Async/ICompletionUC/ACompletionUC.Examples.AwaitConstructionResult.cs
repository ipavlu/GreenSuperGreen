using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class AwaitConstructionResult : ACompletionUC<AwaitConstructionResult, bool>
		{
			public AwaitConstructionResult()
			{
				Trace.WriteLine("some work to be done");

				SetCompletion(true);//result of the operation
			}
		}

		[Test]
		public async Task ExampleAwaitConstructionResultTest()
		{
			Assert.IsTrue(await new AwaitConstructionResult());

			//ACompletionUC is built on awaitable interfaces, ICompletionUC is complete .net awaitable
			Assert.IsTrue(await new AwaitConstructionResult()); await (new AwaitConstruction() as ICompletionUC);


			ICompletionUC<bool> icpl = new AwaitConstructionResult();
			//ACompletionUC is built on awaitable interfaces, ICompletionUC is complete .net awaitable
			Assert.IsTrue(await icpl);
		}
	}
}
