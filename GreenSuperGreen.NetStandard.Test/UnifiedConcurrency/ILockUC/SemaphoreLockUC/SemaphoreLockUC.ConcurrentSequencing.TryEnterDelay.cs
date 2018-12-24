using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	public class SemaphoreLockUCTryEnterDelayTest
	{
		//sequencer points between test code and tested production code
		private enum ConcurrentTryEnterDelaySequencingPhase
		{
			Begin,
			Entry,
			Locked,
			LockedNotify,
			End,
			EnteringSimpleLock
		}

		private static int StepConcurrentSequencing;

		private static void ConcurrentSequencingWorker(ILockUC Lock, ISequencerUC sequencer)
		{
			sequencer.Point(SeqPointTypeUC.Match, ConcurrentTryEnterDelaySequencingPhase.Begin);
			sequencer.Point(SeqPointTypeUC.Notify, ConcurrentTryEnterDelaySequencingPhase.EnteringSimpleLock, Interlocked.Increment(ref StepConcurrentSequencing));
			using (EntryBlockUC entry = Lock.TryEnter(150))
			{
				sequencer.PointArg(SeqPointTypeUC.Match, ConcurrentTryEnterDelaySequencingPhase.Entry, entry.HasEntry);
				if (entry.HasEntry)
				{
					sequencer.Point(SeqPointTypeUC.Match, ConcurrentTryEnterDelaySequencingPhase.Locked, Interlocked.Decrement(ref StepConcurrentSequencing));
					sequencer.Point(SeqPointTypeUC.Notify, ConcurrentTryEnterDelaySequencingPhase.LockedNotify, Interlocked.Add(ref StepConcurrentSequencing, 0));
				}
			}
			sequencer.Point(SeqPointTypeUC.Match, ConcurrentTryEnterDelaySequencingPhase.End);
		}

		[Test]
		public async Task ConcurrentSequencingTryEnterDelay()
		{
			StepConcurrentSequencing = 0;

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(ConcurrentTryEnterDelaySequencingPhase.Begin, new StrategyOneOnOneUC())
			.Register(ConcurrentTryEnterDelaySequencingPhase.EnteringSimpleLock, new StrategyOneOnOneUC())
			.Register(ConcurrentTryEnterDelaySequencingPhase.Entry, new StrategyOneOnOneUC())
			.Register(ConcurrentTryEnterDelaySequencingPhase.Locked, new StrategyOneOnOneUC())
			.Register(ConcurrentTryEnterDelaySequencingPhase.LockedNotify, new StrategyOneOnOneUC())
			.Register(ConcurrentTryEnterDelaySequencingPhase.End, new StrategyOneOnOneUC())
			;

			//StrategyOneOnOneUC each production code point(per thread) is matched to unit test point
			//that is per point and per thread in production code

			ILockUC Lock = new SemaphoreLockUC();

			//start first worker
			sequencer.Run(seq => ConcurrentSequencingWorker(Lock, sequencer));
			//await first worker to get to Begin point
			var Begin1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.Begin);
			//first worker at Begin point


			//start second worker
			sequencer.Run(seq => ConcurrentSequencingWorker(Lock, sequencer));
			//await second worker to get to Begin point
			var Begin2 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.Begin);
			//second worker at Begin point

			//allow first worker to continue to Locked point
			Begin1.Complete();
			//Begin1.Fail(new Exception("Hmmmm"));
			var notify1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.EnteringSimpleLock);

			var entry1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.Entry);
			Assert.AreEqual(true, entry1.ProductionArg);
			entry1.Complete();

			//await first worker to get to Locked point
			var Locked1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.Locked);
			Assert.AreEqual(0, Locked1.ProductionArg);
			//first worker at Locked point

			//spinlock is locked, first worker has exclusive access

			//allow second worker to continue to Locked point it will spin until lock get opened
			Begin2.Complete();

			//this helps us to ensure we do not need thread waiting,
			//notification are in production code completed awaiters,
			//immediatelly executiong what is next after the await on notification,
			//so it is known to be actively running thread!
			var notify2 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.EnteringSimpleLock);
			//second worker has entered spinlock as is spinning!

			var entry2 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.Entry);
			Assert.AreEqual(false, entry2.ProductionArg);
			entry2.Complete();

			//let first worker leave spin lock
			Locked1.Complete();

			//first thread is leaving the spinlock, but before leaving step value will be recorderd
			//second thread is spinning to get inside
			//LockedNotify can be checked if the recorded step value was 1 => thread 1 inside, thread 2 attempting to get inside
			var LockedNotify1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.LockedNotify);
			Assert.AreEqual(1, LockedNotify1.ProductionArg); //this is checking concurrency correctness, as threads were highly probably active during checking


			//await first worker at End point
			var End1 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.End);
			//first worker released spinlock
			End1.Complete();
			//first worker done

			//await second worker at the End point
			var End2 = await sequencer.TestPointAsync(ConcurrentTryEnterDelaySequencingPhase.End);
			//second worker released spinlock
			End2.Complete();
			//second worker done

			await sequencer.WhenAll();

			sequencer.TryReThrowException();
		}
	}
}
