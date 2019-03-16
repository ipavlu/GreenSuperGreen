using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class AsyncLockUCTryEnterDelay : ATestingJobAsync, ITestingJob
	{
		public int Delay { get; }
		public AsyncLockUCTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

		private IAsyncLockUC Lock { get; } = new AsyncLockUC();

		protected override async Task<bool> ExclusiveAccess()
		{
			using (EntryBlockUC entry = await Lock.TryEnter(Delay))
			{
				if (!entry.HasEntry)
				{
					return true;//no entry, keep trying
				}
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		/// <summary> About 15 seconds </summary>
		[Test]
		public async Task AsyncLockUCTryEnterDelayTest()
		{
			using (ITestingJob job = new AsyncLockUCTryEnterDelay(10000, 1500))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
