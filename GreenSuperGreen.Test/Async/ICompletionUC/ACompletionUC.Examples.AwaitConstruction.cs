using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming


namespace GreenSuperGreen.Async.Test
{
	public partial class ACompletionUCTest
	{
		private class AwaitConstruction : ACompletionUC<AwaitConstruction>
		{
			public AwaitConstruction()
			{
				Trace.WriteLine("some work to be done");
				SetCompletion();
			}
		}

		[Test]
		public async Task ExampleAwaitConstructionTest()
		{
			await new AwaitConstruction();

			//ACompletionUC is built on awaitable interfaces, ICompletionUC is complete .net awaitable
			await (new AwaitConstruction() as ICompletionUC);

			ICompletionUC icpl = new AwaitConstruction();

			//ACompletionUC is built on awaitable interfaces, ICompletionUC is complete .net awaitable
			await icpl;
		}
	}
}
