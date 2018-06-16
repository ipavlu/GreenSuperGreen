using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	public class LockUCEnterTest
	{
		//sequencer points between test code and tested production code
		private enum ConcurrentSequencingEnterPhase
		{
			Begin,
			Locked,
			LockedNotify,
			End,
			EnteringSimpleLock
		}

		private static int StepConcurrentSequencing;

		private static void ConcurrentSequencingEnterWorker(ILockUC Lock, ISequencerUC sequencer)
		{
			sequencer.Point(SeqPointTypeUC.Match, ConcurrentSequencingEnterPhase.Begin);
			sequencer.Point(SeqPointTypeUC.Notify, ConcurrentSequencingEnterPhase.EnteringSimpleLock, Interlocked.Increment(ref StepConcurrentSequencing));
			using (Lock.Enter())
			{
				sequencer.Point(SeqPointTypeUC.Match, ConcurrentSequencingEnterPhase.Locked, Interlocked.Decrement(ref StepConcurrentSequencing));
				sequencer.Point(SeqPointTypeUC.Notify, ConcurrentSequencingEnterPhase.LockedNotify, Interlocked.Add(ref StepConcurrentSequencing, 0));
			}
			sequencer.Point(SeqPointTypeUC.Match, ConcurrentSequencingEnterPhase.End);
		}

		[Test]
		public async Task ConcurrentSequencingEnter()
		{
			StepConcurrentSequencing = 0;

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(ConcurrentSequencingEnterPhase.Begin, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingEnterPhase.EnteringSimpleLock, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingEnterPhase.Locked, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingEnterPhase.LockedNotify, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingEnterPhase.End, new StrategyOneOnOneUC())
			;

			//StrategyOneOnOneUC each production code point(per thread) is matched to unit test point
			//that is per point and per thread in production code

			LockUC Lock = new LockUC();

			//start first worker
			sequencer.Run(seq => ConcurrentSequencingEnterWorker(Lock, sequencer));
			//await first worker to get to Begin point
			var Begin1 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.Begin);
			//first worker at Begin point


			//start second worker
			sequencer.Run(seq => ConcurrentSequencingEnterWorker(Lock, sequencer));
			//await second worker to get to Begin point
			var Begin2 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.Begin);
			//second worker at Begin point

			//allow first worker to continue to Locked point
			Begin1.Complete();
			//Begin1.Fail(new Exception("Hmmmm"));
			var notify1 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.EnteringSimpleLock);
			//await first worker to get to Locked point
			var Locked1 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.Locked);
			Assert.AreEqual(0, Locked1.ProductionArg);
			//first worker at Locked point

			//spinlock is locked, first worker has exclusive access

			//allow first worker to continue to Locked point it will spin until lock get opened
			Begin2.Complete();

			//this helps us to ensure we do not need thread waiting,
			//notification are in production code completed awaiters,
			//immediatelly executiong what is next after the await on notification,
			//so it is known to be actively running thread!
			var notify2 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.EnteringSimpleLock);
			//second worker has entered spinlock as is spinning!

			//let first worker leave spin lock
			Locked1.Complete();

			//first thread is leaving the spinlock, but before leaving step value will be recorderd
			//second thread is spinning to get inside
			//LockedNotify can be checked if the recorded step value was 1 => thread 1 inside, thread 2 attempting to get inside
			var LockedNotify1 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.LockedNotify);
			Assert.AreEqual(1, LockedNotify1.ProductionArg); //this is checking concurrency correctness, as threads were highly probably active during checking


			//await first worker at End point
			var End1 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.End);
			//first worker released spinlock
			End1.Complete();
			//first worker done

			//await second worker at Locked point
			var Locked2 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.Locked);
			//let second worker to continue to End point
			Locked2.Complete();

			var LockedNotify2 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.LockedNotify);
			Assert.AreEqual(0, LockedNotify2.ProductionArg);

			//await second worker at the End point
			var End2 = await sequencer.TestPointAsync(ConcurrentSequencingEnterPhase.End);
			//second worker released spinlock
			End2.Complete();
			//second worker done

			await sequencer.WhenAll();

			sequencer.TryReThrowException();
		}
	}
}
