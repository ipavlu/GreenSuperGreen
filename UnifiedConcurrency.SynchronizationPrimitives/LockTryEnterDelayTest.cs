using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class LockTryEnterDelay : ATestingJob, ITestingJob
	{
		public int Delay { get; }
		public LockTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

		private ILockUC Lock { get; } = new LockUC();

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.TryEnter(Delay))
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
		[Test]
		public async Task LockTryEnterDelayTest()
		{
			using (ITestingJob job = new LockTryEnterDelay(1000000, 15))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
