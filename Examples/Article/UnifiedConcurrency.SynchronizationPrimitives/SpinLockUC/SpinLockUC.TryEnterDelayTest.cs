using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class SpinLockUCTryEnterDelay : ATestingJob, ITestingJob
	{
		public int Delay { get; }
		public SpinLockUCTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

		private ILockUC Lock { get; } = new SpinLockUC();

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
		/// <summary> About 30 seconds </summary>
		[Test]
		public async Task SpinLockUCTryEnterDelayTest()
		{
			using (ITestingJob job = new SpinLockUCTryEnterDelay(10000, 15))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
