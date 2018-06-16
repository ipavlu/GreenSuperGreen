using System;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async.Test
{
	[TestFixture]
	public class TaskCompletionSourceSynchronousTests
	{
		[ThreadStatic]
		private static string ThreadStaticWorker;

		private enum TCS
		{
			ReceiverInitialize,
			ReceiverRecord,
			MessangerSetResult
		}


		private void Messanger(ISequencerUC sequencer, TaskCompletionSource<object> tcs)
		{
			ThreadStaticWorker = nameof(Messanger);//thread static to detect Receiver is taking same thread
			sequencer.Point(SeqPointTypeUC.Notify, TCS.MessangerSetResult);
			tcs.SetResult(null);
			//before the thread is returned to thread pool, the thread static field is cleaned,
			ThreadStaticWorker = null;
		}

		private async Task Receiver(ISequencerUC sequencer, TaskCompletionSource<object> tcs)
		{
			sequencer.Point(SeqPointTypeUC.Notify, TCS.ReceiverInitialize);

			await tcs.Task;

			sequencer.Point(SeqPointTypeUC.Notify, TCS.ReceiverRecord, ThreadStaticWorker);//saving Worker state

			if (ThreadStaticWorker == nameof(Messanger))
			{
				//$"{nameof(Messanger)} is now working for us synchronously!";
			}
		}

		[Test]
		public async Task EnslaveAndDelayMessangerTest()
		{
			//The danger is hidden in using the messangers thread to run continuation,
			//for semaphores in messanger/receiver blocks it can easily create deadlock,

			//For monitor lock in messanger/receiver blocks it should mostly work well,
			// recursivness takes care of it.

			//For UI it is highly dangerous and unpredictable!
			//If messanger is executed from UI, it will most often execute continuation synchronously on UI.
			//BUT!
			//In certain cases TaskCompletionSource can decide to run continuation asynchronously on UI.
			//Same as if a delegate would not be executed directly but through BeginInvoke scheduled to queue
			//and executed later.
			//It is a breaking change to subsequent code, that might expect something will be already done before
			//subsequent code is executed.
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(TCS.ReceiverInitialize, new StrategyOneOnOneUC())
			.Register(TCS.ReceiverRecord, new StrategyOneOnOneUC())
			.Register(TCS.MessangerSetResult, new StrategyOneOnOneUC())
			;

			sequencer.Run(seq => Receiver(seq, tcs));//run Receiver in its own thread
			await sequencer.TestPointAsync(TCS.ReceiverInitialize);//receiver was running and is now awaiting tcs.Task

			sequencer.Run(seq => Messanger(seq, tcs));//run Messanger in its own thread
			await sequencer.TestPointAsync(TCS.MessangerSetResult);//messanger SetResult executed

			var worker = await sequencer.TestPointAsync(TCS.ReceiverRecord);//receiver was running and is now awaiting tcs.Task

			//Messanger was executing code inside Receiver synchronously!
			Assert.AreEqual(worker.ProductionArg, nameof(Messanger));

			await sequencer.WhenAll();// wait for all tasks to complete
			Assert.DoesNotThrow(() => sequencer.TryReThrowException());
		}

		[Test]
		public async Task DoNotEnslaveAndDelayMessangerTest()
		{
			//The main difference, here as it should be used, but did not exist till .Net 4.5
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(TCS.ReceiverInitialize, new StrategyOneOnOneUC())
			.Register(TCS.ReceiverRecord, new StrategyOneOnOneUC())
			.Register(TCS.MessangerSetResult, new StrategyOneOnOneUC())
			;

			sequencer.Run(seq => Receiver(seq, tcs));//run Receiver in own thread
			await sequencer.TestPointAsync(TCS.ReceiverInitialize);//receiver was running and is awaiting tcs now

			sequencer.Run(seq => Messanger(seq, tcs));//run Messanger in own thread
			await sequencer.TestPointAsync(TCS.MessangerSetResult);//messanger SetResult executed

			var worker = await sequencer.TestPointAsync(TCS.ReceiverRecord);//get info about Receivers thread

			//Messanger was not executing code inside Receiver synchronously!
			Assert.AreNotEqual(worker.ProductionArg, nameof(Messanger));

			await sequencer.WhenAll();// wait for all tasks to complete
			Assert.DoesNotThrow(() => sequencer.TryReThrowException());
		}
	}
}
