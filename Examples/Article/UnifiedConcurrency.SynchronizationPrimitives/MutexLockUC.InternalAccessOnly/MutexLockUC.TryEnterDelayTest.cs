using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	/// <summary>
	/// MutexLockUC is an accessed as internal class, serves only as example and for benchmarking,
	/// because Monitor class requires thread affinity between thread enter and thread exit points
	/// and thread affinity is not supported by UnifiedConcurrency.
	/// Here the thread affinity is handled so it does not bother, but be aware if you change this example.
	/// Also in any other project MutexLockUC wont be accessible.
	/// </summary>
	public sealed class MutexLockUCTryEnterDelay : ATestingJob, ITestingJob
	{
		public int Delay { get; }
		public MutexLockUCTryEnterDelay(int count, int delay) : base(count) { Delay = delay; }

#pragma warning disable 618
		private ILockUC Lock { get; } = new MutexLockUC();
#pragma warning restore 618

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
		public async Task MutexLockUCTryEnterDelayTest()
		{
			using (ITestingJob job = new MutexLockUCTryEnterDelay(10000, 15))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
