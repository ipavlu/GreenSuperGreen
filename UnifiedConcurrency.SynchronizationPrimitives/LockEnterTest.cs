using System;
using System.CodeDom;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class LockEnter : ATestingJob, ITestingJob
	{
		public LockEnter(int count) : base(count) { }

		private ISimpleLockUC Lock { get; } = new LockUC();

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.Enter())
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
		public async Task LockEnterTest()
		{
			using (ITestingJob job = new LockEnter(100000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
