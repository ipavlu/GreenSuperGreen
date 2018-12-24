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
		private enum ConcurrentSequencingPhaseConditional
		{
			Begin,
			Locked,
			LockedNotify,
			End,
			EnteringSpinLock
		}

		private static int StepConcurrentSequencingConditional;

		private static void ConcurrentSequencingConditionalWorker(ISequencerUC sequencer, ILockUC spinLock)
		{
			ConditionalSequencerUC.Point(sequencer, SeqPointTypeUC.Match, ConcurrentSequencingPhaseConditional.Begin);
			ConditionalSequencerUC.Point(sequencer, SeqPointTypeUC.Notify, ConcurrentSequencingPhaseConditional.EnteringSpinLock, Interlocked.Increment(ref StepConcurrentSequencingConditional));
			using (spinLock.Enter())
			{
				ConditionalSequencerUC.Point(sequencer, SeqPointTypeUC.Match, ConcurrentSequencingPhaseConditional.Locked, Interlocked.Decrement(ref StepConcurrentSequencingConditional));
				ConditionalSequencerUC.Point(sequencer, SeqPointTypeUC.Notify, ConcurrentSequencingPhaseConditional.LockedNotify, Interlocked.Add(ref StepConcurrentSequencingConditional, 0));
			}
			ConditionalSequencerUC.Point(sequencer, SeqPointTypeUC.Match, ConcurrentSequencingPhaseConditional.End);
		}

		//[Ignore("Ignored until mix of debug/release in production/unit test dll will solved")]



		[Ignore("Ignored until mix of debug/release in production/unit test dll will be solved")]
		[Test()]
		public async Task ConcurrentSequencingConditional()
		{
			StepConcurrentSequencingConditional = 0;

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(ConcurrentSequencingPhaseConditional.Begin, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseConditional.EnteringSpinLock, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseConditional.Locked, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseConditional.LockedNotify, new StrategyOneOnOneUC())
			.Register(ConcurrentSequencingPhaseConditional.End, new StrategyOneOnOneUC())
			;

			//StrategyOneOnOneUC each production code point(per thread) is matched to unit test point
			//that is per point and per thread in production code
			ILockUC spinlock = new SpinLockUC();

			//start first worker
			sequencer.Run(spinlock, ConcurrentSequencingConditionalWorker);
			//await first worker to get to Begin point
			var Begin1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.Begin);
			//first worker at Begin point

			//start second worker
			sequencer.Run(spinlock, ConcurrentSequencingConditionalWorker);
			//await second worker to get to Begin point
			var Begin2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.Begin);
			//second worker at Begin point

			//allow first worker to continue to Locked point
			Begin1.Complete();
			//Begin1.Fail(new Exception("Hmmmm"));
			var notify1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.EnteringSpinLock);
			//await first worker to get to Locked point
			var Locked1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.Locked);
			Assert.AreEqual(0, Locked1.ProductionArg);
			//first worker at Locked point

			//spinlock is locked, first worker has exclusive access

			//allow first worker to continue to Locked point it will spin until lock get opened
			Begin2.Complete();

			//this helps us to ensure we do not need thread waiting,
			//notification are in production code completed awaiters,
			//immediatelly executiong what is next after the await on notification,
			//so it is known to be actively running thread!
			var notify2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.EnteringSpinLock);
			//second worker has entered spinlock as is spinning!

			//let first worker leave spin lock
			Locked1.Complete();
			//await first worker at End point
			//first thread is leaving the spinlock, but before leaving step value will be recorderd
			//second thread is spinning to get inside
			//LockedNotify can be checked if the recorded step value was 1 => thread 1 inside, thread 2 attempting to get inside
			var LockedNotify1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.LockedNotify);
			Assert.AreEqual(1, LockedNotify1.ProductionArg); //this is checking concurrency correctness, as threads were highly probably active during checking


			var End1 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.End);
			//first worker released spinlock
			End1.Complete();
			//first worker done

			//await second worker at Locked point
			var Locked2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.Locked);
			//let second worker to continue to End point
			Locked2.Complete();

			var LockedNotify2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.LockedNotify);
			Assert.AreEqual(0, LockedNotify2.ProductionArg);

			//await second worker at the End point
			var End2 = await sequencer.TestPointAsync(ConcurrentSequencingPhaseConditional.End);
			//second worker released spinlock
			End2.Complete();
			//second worker done

			await sequencer.WhenAll();

			sequencer.TryReThrowException();
		}
	}
}
