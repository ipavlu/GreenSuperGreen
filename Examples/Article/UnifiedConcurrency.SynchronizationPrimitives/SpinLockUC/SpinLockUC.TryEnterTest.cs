using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class SpinLockUCTryEnter : ATestingJob, ITestingJob
	{
		public SpinLockUCTryEnter(int count) : base(count) { }

		private ILockUC Lock { get; } = new SpinLockUC();

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				if (!entry.HasEntry) return true;//no entry, keep trying
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		/// <summary> About 30 seconds </summary>
		[Test]
		public async Task SpinLockUCTryEnterTest()
		{
			using (ITestingJob job = new SpinLockUCTryEnter(10000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
