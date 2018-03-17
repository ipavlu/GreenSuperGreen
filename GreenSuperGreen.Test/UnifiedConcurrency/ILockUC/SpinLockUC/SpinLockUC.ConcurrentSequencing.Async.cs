using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	public partial class SpinLockUCTest
	{
		//sequencer points between test code and tested production code
		private enum ConcurrentSequencingPhaseAsync
		{
			Begin,
			Locked,
			LockedNotify,
			End,
			EnteringSpinLock
		}

		private static int StepConcurrentSequencingAsync;

		private static async Task ConcurrentSequencingAsyncWorker(SpinLockUC spinLock, ISequencerUC sequencer)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, ConcurrentSequencingPhaseAsync.Begin);
			await sequencer.PointAsync(SeqPointTypeUC.Notify, ConcurrentSequencingPhaseAsync.EnteringSpinLock, Interlocked.Increment(ref StepConcurrentSequencingAsync));
			using (spinLock.Enter())
			{
				await sequencer.PointAsync(SeqPointTypeUC.Match, ConcurrentSequencingPhaseAsync.Locked, Interlocked.Decrement(ref StepConcurrentSequencingAsync));
				await sequencer.PointAsync(SeqPointTypeUC.Notify, ConcurrentSequencingPhaseAsync.LockedNotify, Interlocked.Add(ref StepConcurrentSequencingAsync, 0));
			}
			await sequencer.PointAsync(SeqPointTypeUC.Match, ConcurrentSequencingPhaseAsync.End);
		}

		[Test]
		public async Task ConcurrentSequencingAsync()
		{
			StepConcurrentSequencingAsync = 0;

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(ConcurrentSequencingPhaseAsync.Begin, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseAsync.EnteringSpinLock, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseAsync.Locked, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseAsync.LockedNotify, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseAsync.End, new StrategyOneOnOneUC())
			;

			//StrategyOneOnOneUC each production code point(per thread) is matched to unit test point
			//that is per point and per thread in production code

			SpinLockUC spinlock = new SpinLockUC();

			//start first worker
			sequencer.Run(seq => ConcurrentSequencingAsyncWorker(spinlock, seq));
			//await first worker to get to Begin point
			var Begin1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.Begin);
			//first worker at Begin point


			//start second worker
			sequencer.Run(seq => ConcurrentSequencingAsyncWorker(spinlock, seq));
			//await second worker to get to Begin point
			var Begin2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.Begin);
			//second worker at Begin point
			
			//allow first worker to continue to Locked point
			Begin1.Complete();
			//Begin1.Fail(new Exception("Hmmmm"));
			var notify1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.EnteringSpinLock);
			//await first worker to get to Locked point
			var Locked1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.Locked);
			Assert.AreEqual(0, Locked1.ProductionArg);
			//first worker at Locked point

			//spinlock is locked, first worker has exclusive access

			//allow first worker to continue to Locked point it will spin until lock get opened
			Begin2.Complete();

			//this helps us to ensure we do not need thread waiting,
			//notification are in production code completed awaiters,
			//immediatelly executiong what is next after the await on notification,
			//so it is known to be actively running thread!
			var notify2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.EnteringSpinLock);
			//second worker has entered spinlock as is spinning!

			//let first worker leave spin lock
			Locked1.Complete();

			//first thread is leaving the spinlock, but before leaving step value will be recorderd
			//second thread is spinning to get inside
			//LockedNotify can be checked if the recorded step value was 1 => thread 1 inside, thread 2 attempting to get inside
			var LockedNotify1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.LockedNotify);
			Assert.AreEqual(1, LockedNotify1.ProductionArg); //this is checking concurrency correctness, as threads were highly probably active during checking

			//await first worker at End point
			var End1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.End);
			//first worker released spinlock
			End1.Complete();
			//first worker done

			//await second worker at Locked point
			var Locked2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.Locked);
			//let second worker to continue to End point
			Locked2.Complete();

			var LockedNotify2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.LockedNotify);
			Assert.AreEqual(0, LockedNotify2.ProductionArg);

			//await second worker at the End point
			var End2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseAsync.End);
			//second worker released spinlock
			End2.Complete();
			//second worker done

			await sequencer.WhenAll();

			sequencer.TryReThrowException();
		}
	}
}
