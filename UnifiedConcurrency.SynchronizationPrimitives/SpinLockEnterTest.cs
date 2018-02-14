using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class SpinLockEnter : ATestingJob, ITestingJob
	{
		public SpinLockEnter(int count) : base(count) { }

		private ISimpleLockUC Lock { get; } = new SpinLockUC();

		protected override bool ExclusiveAccess()
		{
			using (Lock.Enter())
			{
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		[Test]
		public async Task SpinLockEnterTest()
		{
			using (ITestingJob job = new SpinLockEnter(1000000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
