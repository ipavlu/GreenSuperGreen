using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class AsyncLockTryEnterDelay : AAsyncTestingJob, ITestingJob
	{
		public int Delay { get; }
		public AsyncLockTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

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
		public async Task AsyncLockTryEnterDelayTest()
		{
			using (ITestingJob job = new AsyncLockTryEnterDelay(1000000, 1500))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
