using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class TicketSpinLockEnter : ATestingJob, ITestingJob
	{
		public TicketSpinLockEnter(int count) : base(count) { }

		private ISimpleLockUC Lock { get; } = new TicketSpinLockUC();

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
		public async Task TicketSpinLockEnterTest()
		{
			using (ITestingJob job = new TicketSpinLockEnter(1000000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
