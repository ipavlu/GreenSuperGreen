using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class SemaphoreLockUCEnter : ATestingJob, ITestingJob
	{
		public SemaphoreLockUCEnter(int count) : base(count) { }

		private ILockUC Lock { get; } = new SemaphoreLockUC();

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
		public async Task SemaphoreLockUCEnterTest()
		{
			using (ITestingJob job = new SemaphoreLockUCEnter(10000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
