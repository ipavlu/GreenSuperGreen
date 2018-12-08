using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class AsyncLockEnter : AAsyncTestingJob, ITestingJob
	{
		public AsyncLockEnter(int count) : base(count) { }

		private IAsyncLockUC Lock { get; } = new AsyncLockUC();

		protected override async Task<bool> ExclusiveAccess()
		{
			using (EntryBlockUC entry = await Lock.Enter())
			{
				if (!entry.HasEntry) throw new Exception("hmmmmmmmmm");
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		[Test]
		public async Task AsyncLockEnterTest()
		{
			using (ITestingJob job = new AsyncLockEnter(1000000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
