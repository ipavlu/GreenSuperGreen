using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class AsyncSemaphoreSlimLockUCTryEnter : ATestingJobAsync, ITestingJob
	{
		public AsyncSemaphoreSlimLockUCTryEnter(int count) : base(count) { }

		private IAsyncLockUC Lock { get; } = new AsyncSemaphoreSlimLockUC();

		protected override async Task<bool> ExclusiveAccess()
		{
			using (EntryBlockUC entry = await Lock.TryEnter())
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
		public async Task AsyncSemaphoreSlimLockUCTryEnterTest()
		{
			using (ITestingJob job = new AsyncSemaphoreSlimLockUCTryEnter(10000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
