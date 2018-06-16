using System;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing.Test
{
	[TestFixture]
	public class TimerProcessorTest
	{
		private enum Steps
		{
			Notify
		}

		[Test]
		public async Task BasicRegisterTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			var t = await timerProcessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(100)).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			t = await timerProcessor.UnRegisterAsync(tcs).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);
		}

		[Test]
		public async Task BasicRegisterResultTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			var t = await timerProcessor.RegisterResultAsync(TimeSpan.FromMilliseconds(100), new object()).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			t = await timerProcessor.UnRegisterAsync(tcs).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);
		}

		[Test]
		public async Task CompletedWithoutTimoutRegisterTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(OrderedExpiryItemsSequencer.TryExpireBegin, new StrategyOneOnOneUC())
			;

			var timerProcessor = new TimerProcessor(10, new RealTimeSource(), sequencer);

			var t = await timerProcessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(10000)).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			//OrderedExpiryItemsSequencer.TryExpireBegin is Match point, it wont proceed without point setting to Complete() state
			tcs.SetResult(null);
			Assert.IsTrue(tcs.Task.IsCompleted);
			Assert.IsFalse(tcs.Task.IsCanceled);
			Assert.IsFalse(tcs.Task.IsFaulted);

			t = await timerProcessor.UnRegisterAsync(tcs).WrapIntoTask();
			var tcs2 = t.Result;
			Assert.AreEqual(tcs, tcs2);

			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.IsTrue(tcs2.Task.IsCompleted);
			Assert.IsFalse(tcs2.Task.IsCanceled);
			Assert.IsFalse(tcs2.Task.IsFaulted);
		}

		[Test]
		public async Task CompletedWithoutTimoutRegisterResultTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(OrderedExpiryItemsSequencer.TryExpireBegin, new StrategyOneOnOneUC())
			;

			var timerProcessor = new TimerProcessor(10, new RealTimeSource(), sequencer);

			var t = await timerProcessor.RegisterResultAsync(TimeSpan.FromMilliseconds(10000), new object()).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			//OrderedExpiryItemsSequencer.TryExpireBegin is Match point, it wont proceed without point setting to Complete() state
			tcs.SetResult(null);
			Assert.IsTrue(tcs.Task.IsCompleted);
			Assert.IsFalse(tcs.Task.IsCanceled);
			Assert.IsFalse(tcs.Task.IsFaulted);

			t = await timerProcessor.UnRegisterAsync(tcs).WrapIntoTask();
			var tcs2 = t.Result;
			Assert.AreEqual(tcs, tcs2);

			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.IsTrue(tcs2.Task.IsCompleted);
			Assert.IsFalse(tcs2.Task.IsCanceled);
			Assert.IsFalse(tcs2.Task.IsFaulted);
		}

		[Test]
		public async Task CompletedWithTimoutRegisterTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			var t = await timerProcessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(25)).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.CatchAsync<TaskCanceledException>(async () => await tcs.Task);
		}

		[Test]
		public async Task CompletedWithTimoutRegisterResultTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			var t = await timerProcessor.RegisterResultAsync(TimeSpan.FromMilliseconds(25), new object()).WrapIntoTask();
			var tcs = t.Result;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.IsNotNull(await tcs.Task);
		}

		[Test]
		public async Task ConcurrentRegisterTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()

			.Register(TimerProcessorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var timeprocessor = new TimerProcessor(10, new RealTimeSource(), sequencer);


			var tt = Task.Run(() => timeprocessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(10000)).WrapIntoTask());

			var rStatus = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterStatus);
			Assert.AreEqual(TimerProcessorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
			var processingResult = (TimerProcessor.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive | TimerProcessorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessing.ProductionArg);


			Task<TaskCompletionSource<object>> t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			TaskCompletionSource<object> tcs = t.Result;


			for (int i = 0; i < 1; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => timeprocessor.UnRegisterAsync(tcs).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterStatus);
				Assert.AreEqual(TimerProcessorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(true, rActiveProcesingCycle.ProductionArg);

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				{
					await sequencer.TestPointAsync(Steps.Notify);
				}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task ConcurrentRegisterResultTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()

			.Register(TimerProcessorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var timeprocessor = new TimerProcessor(10, new RealTimeSource(), sequencer);


			var tt = Task.Run(() => timeprocessor.RegisterResultAsync(TimeSpan.FromMilliseconds(10000), new object()).WrapIntoTask());

			var rStatus = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterStatus);
			Assert.AreEqual(TimerProcessorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
			var processingResult = (TimerProcessor.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive | TimerProcessorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessing.ProductionArg);


			Task<TaskCompletionSource<object>> t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			TaskCompletionSource<object> tcs = t.Result;


			for (int i = 0; i < 1; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => timeprocessor.UnRegisterAsync(tcs).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterStatus);
				Assert.AreEqual(TimerProcessorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(true, rActiveProcesingCycle.ProductionArg);

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				{
					await sequencer.TestPointAsync(Steps.Notify);
				}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task Concurrent2RegisterTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()

			.Register(TimerProcessorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var timeprocessor = new TimerProcessor(10, new RealTimeSource(), sequencer);


			var tt = Task.Run(() => timeprocessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(10000)).WrapIntoTask());

			var rStatus = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterStatus);
			Assert.AreEqual(TimerProcessorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
			var processingResult = (TimerProcessor.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive | TimerProcessorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessing.ProductionArg);


			Task<TaskCompletionSource<object>> t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);
			TaskCompletionSource<object> tcs = t.Result;

			for (int i = 0; i < 10; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				//var callBackProcessing = 
				await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				//callBackProcessing.Complete();

				//var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				//var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				//Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				//var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				//exclusiveProcessingCycle.Complete();

				//var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				//beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => timeprocessor.UnRegisterAsync(tcs).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterStatus);
				Assert.AreEqual(TimerProcessorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(false, rActiveProcesingCycle.ProductionArg);

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(false, actionProcessingCycle.ProductionArg);

				//var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				//Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task Concurrent2RegisterResultTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()

			.Register(TimerProcessorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TimerProcessorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TimerProcessorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var timeprocessor = new TimerProcessor(10, new RealTimeSource(), sequencer);


			var tt = Task.Run(() => timeprocessor.RegisterResultAsync(TimeSpan.FromMilliseconds(10000), new object()).WrapIntoTask());

			var rStatus = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterStatus);
			Assert.AreEqual(TimerProcessorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TimerProcessorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
			var processingResult = (TimerProcessor.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive | TimerProcessorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessing.ProductionArg);


			Task<TaskCompletionSource<object>> t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);
			TaskCompletionSource<object> tcs = t.Result;

			for (int i = 0; i < 10; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Activate | TimerProcessorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				//var callBackProcessing = 
				await sequencer.TestPointAsync(TimerProcessorSequencer.CallBackProcessing);
				//callBackProcessing.Complete();

				//var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				//var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				//Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				//var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				//exclusiveProcessingCycle.Complete();

				//var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				//beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => timeprocessor.UnRegisterAsync(tcs).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterStatus);
				Assert.AreEqual(TimerProcessorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(false, rActiveProcesingCycle.ProductionArg);

				var processingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.Processing);
				var processingCycleResult = (TimerProcessor.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TimerProcessor.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(false, actionProcessingCycle.ProductionArg);

				//var actionProcessingCountCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.ActionsProcessingCount);
				//Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TimerProcessorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TimerProcessorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TimerProcessorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}



		[Test]
		public async Task DisposeBasicRegisterTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			timerProcessor.Dispose();
			Task t = await timerProcessor.Disposed.WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			TaskCompletionSource<object> ttt = new TaskCompletionSource<object>();
			Assert.ThrowsAsync<ObjectDisposedException>(async () => await timerProcessor.RegisterAsync<object>(TimeSpan.FromMilliseconds(10000)));
			Assert.ThrowsAsync<ObjectDisposedException>(async () => await timerProcessor.UnRegisterAsync(ttt));
		}

		[Test]
		public async Task DisposeBasicRegisterResultTest()
		{
			var timerProcessor = new TimerProcessor(10, new RealTimeSource());

			timerProcessor.Dispose();
			Task t = await timerProcessor.Disposed.WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			TaskCompletionSource<object> ttt = new TaskCompletionSource<object>();
			Assert.ThrowsAsync<ObjectDisposedException>(async () => await timerProcessor.RegisterResultAsync(TimeSpan.FromMilliseconds(10000), new object()));
			Assert.ThrowsAsync<ObjectDisposedException>(async () => await timerProcessor.UnRegisterAsync(ttt));
		}


		[Test]
		[Ignore("Incomplete")]
		public async Task MultipleRegistrationsTest()
		{
			//var TimerProcessor = new TimerProcessor(10);

			//ISequencerUC sequencer =
			//SequencerUC
			//.Construct()
			//.Register(Steps.Notify, new StrategyOneOnOneUC())
			//;

			//Action action = () =>
			//{
			//	sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			//};

			//Task t = await TimerProcessor.RegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//t = await TimerProcessor.RegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//for (int i = 0; i < 10; i++)
			//{
			//	await sequencer.TestPointAsync(Steps.Notify);
			//}

			//t = await TimerProcessor.UnRegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//await sequencer.WhenAll();
			//sequencer.TryReThrowException();
			await Task.CompletedTask;
		}

		[Test]
		[Ignore("Incomplete")]
		public async Task MultipleUnRegistrationsTest()
		{
			//var TimerProcessor = new TimerProcessor(10);

			//ISequencerUC sequencer =
			//SequencerUC
			//.Construct()
			//.Register(Steps.Notify, new StrategyOneOnOneUC())
			//;

			//Action action = () =>
			//{
			//	sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			//};

			//Task t = await TimerProcessor.RegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//for (int i = 0; i < 10; i++)
			//{
			//	await sequencer.TestPointAsync(Steps.Notify);
			//}

			//t = await TimerProcessor.UnRegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//t = await TimerProcessor.UnRegisterAsync(action).WrapIntoTask();
			//Assert.IsTrue(t.IsCompleted);
			//Assert.IsFalse(t.IsCanceled);
			//Assert.IsFalse(t.IsFaulted);

			//await sequencer.WhenAll();
			//sequencer.TryReThrowException();
			await Task.CompletedTask;
		}
	}
}
