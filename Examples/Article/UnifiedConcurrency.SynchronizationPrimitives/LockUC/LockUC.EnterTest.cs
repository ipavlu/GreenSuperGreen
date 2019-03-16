using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class LockUCEnter : ATestingJob, ITestingJob
	{
		public LockUCEnter(int count) : base(count) { }

		private ILockUC Lock { get; } = new LockUC();

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.Enter())
			{
				if (!entry.HasEntry) throw new Exception("should not happen");
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		/// <summary> About 30 seconds </summary>
		[Test]
		public async Task LockUCEnterTest()
		{
			using (ITestingJob job = new LockUCEnter(10000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
