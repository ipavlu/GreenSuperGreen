using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	/// <summary>
	/// MonitorLockUC is an accessed as internal class, serves only as example and for benchmarking,
	/// because Monitor class requires thread affinity between thread enter and thread exit points
	/// and thread affinity is not supported by UnifiedConcurrency.
	/// Here the thread affinity is handled so it does not bother, but be aware if you change this example.
	/// Also in any other project MonitorLockUC wont be accessible.
	/// </summary>
	public sealed class MonitorLockUCEnter : ATestingJob, ITestingJob
	{
		public MonitorLockUCEnter(int count) : base(count) { }

#pragma warning disable 618
		private ILockUC Lock { get; } = new MonitorLockUC();
#pragma warning restore 618

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
		public async Task MonitorLockUCEnterTest()
		{
			using (ITestingJob job = new MonitorLockUCEnter(10000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
