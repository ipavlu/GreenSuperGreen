using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class MonitorTryEnterDelay : ATestingJob, ITestingJob
	{
		public int Delay { get; }
		public MonitorTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

		private ISimpleLockUC Lock { get; } = new MonitorLockUC();

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
		public async Task MonitorTryEnterDelayTest()
		{
			using (ITestingJob job = new MonitorTryEnterDelay(1000000, 15))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
