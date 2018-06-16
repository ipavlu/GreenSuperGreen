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
	public class TickGeneratorTest
	{
		private enum Steps
		{
			Notify
		}

		[Test]
		public async Task BasicTest()
		{
			var tickGenerator = new TickGenerator(10);

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())
			;

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			Task t = await tickGenerator.RegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			for (int i = 0; i < 10; i++) 
			{
				await sequencer.TestPointAsync(Steps.Notify);
			}

			t = await tickGenerator.UnRegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task ConcurrentTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var tickGenerator = new TickGenerator(10, sequencer);

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			Task<Task> tt = Task.Run(()=> tickGenerator.RegisterAsync(action).WrapIntoTask());
			
			var rStatus = await sequencer.TestPointAsync(TickGeneratorSequencer.RegisterStatus);
			Assert.AreEqual(TickGeneratorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TickGeneratorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
			var processingResult = (TickGenerator.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TickGeneratorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TickGeneratorTimerStatus.Activate | TickGeneratorTimerStatus.IsActive | TickGeneratorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessing.ProductionArg);


			Task t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);


			for (int i = 0; i < 10; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
				var processingCycleResult = (TickGenerator.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				{
					await sequencer.TestPointAsync(Steps.Notify);
				}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.Activate | TickGeneratorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
				var processingCycleResult = (TickGenerator.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => tickGenerator.UnRegisterAsync(action).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.UnRegisterStatus);
				Assert.AreEqual(TickGeneratorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(true, rActiveProcesingCycle.ProductionArg);
				
				var actionProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				{
					await sequencer.TestPointAsync(Steps.Notify);
				}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task Concurrent2Test()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.RegisterStatus, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.RegisterActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.UnRegisterStatus, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.UnRegisterActiveProcessing, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.Processing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ExclusiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.BeginActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.EndActiveProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ActionsProcessing, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.ActionsProcessingCount, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.TryUpdateTimerBegin, new StrategyOneOnOneUC())
			.Register(TickGeneratorSequencer.TryUpdateTimerEnd, new StrategyOneOnOneUC())

			.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			//.Register(TickGeneratorSequencer.CallBackProcessing, new StrategyOneOnOneUC())
			;

			var tickGenerator = new TickGenerator(10, sequencer);

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			Task<Task> tt = Task.Run(() => tickGenerator.RegisterAsync(action).WrapIntoTask());

			var rStatus = await sequencer.TestPointAsync(TickGeneratorSequencer.RegisterStatus);
			Assert.AreEqual(TickGeneratorStatus.Operating, rStatus.ProductionArg);
			var rActiveProcesing = await sequencer.TestPointAsync(TickGeneratorSequencer.RegisterActiveProcessing);
			Assert.AreEqual(false, rActiveProcesing.ProductionArg);

			var processing = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
			var processingResult = (TickGenerator.ProcessingResult)processing.ProductionArg;
			Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingResult);

			var exclusiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
			exclusiveProcessing.Complete();

			var beginActiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
			beginActiveProcessing.Complete();

			var actionProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
			actionProcessing.Complete();
			Assert.AreEqual(false, actionProcessing.ProductionArg);

			var updateTimerBegin = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
			updateTimerBegin.Complete();
			Assert.AreEqual(TickGeneratorTimerStatus.None, updateTimerBegin.ProductionArg);

			var updateTimerEnd = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
			updateTimerEnd.Complete();
			Assert.AreEqual(TickGeneratorTimerStatus.Activate | TickGeneratorTimerStatus.IsActive | TickGeneratorTimerStatus.Changed, updateTimerEnd.ProductionArg);

			var endActiveProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
			endActiveProcessing.Complete();
			Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessing.ProductionArg);


			Task t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);


			for (int i = 0; i < 10; i++)
			{
				var callBackProcessing = await sequencer.TestPointAsync(TickGeneratorSequencer.CallBackProcessing);
				callBackProcessing.Complete();

				var processingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
				var processingCycleResult = (TickGenerator.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(true, actionProcessingCycle.ProductionArg);

				var actionProcessingCountCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessingCount);
				Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				{
					await sequencer.TestPointAsync(Steps.Notify);
				}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.Activate | TickGeneratorTimerStatus.IsActive, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}

			for (int i = 0; i < 1; i++)
			{
				//var callBackProcessing = 
				await sequencer.TestPointAsync(TickGeneratorSequencer.CallBackProcessing);
				//callBackProcessing.Complete();

				//var processingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
				//var processingCycleResult = (TickGenerator.ProcessingResult)processingCycle.ProductionArg;
				//Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingCycleResult);

				//var exclusiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
				//exclusiveProcessingCycle.Complete();

				//var beginActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
				//beginActiveProcessingCycle.Complete();

				//ActiveProcessing
				tt = Task.Run(() => tickGenerator.UnRegisterAsync(action).WrapIntoTask());

				var rStatusCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.UnRegisterStatus);
				Assert.AreEqual(TickGeneratorStatus.Operating, rStatusCycle.ProductionArg);
				var rActiveProcesingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.UnRegisterActiveProcessing);
				Assert.AreEqual(false, rActiveProcesingCycle.ProductionArg);

				var processingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.Processing);
				var processingCycleResult = (TickGenerator.ProcessingResult)processingCycle.ProductionArg;
				Assert.AreEqual(TickGenerator.ProcessingResult.Processed, processingCycleResult);

				var exclusiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ExclusiveProcessing);
				exclusiveProcessingCycle.Complete();

				var beginActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.BeginActiveProcessing);
				beginActiveProcessingCycle.Complete();

				var actionProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessing);
				actionProcessingCycle.Complete();
				Assert.AreEqual(false, actionProcessingCycle.ProductionArg);

				//var actionProcessingCountCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.ActionsProcessingCount);
				//Assert.AreEqual(1, actionProcessingCountCycle.ProductionArg);
				//for (int j = 0; j < (int)actionProcessingCountCycle.ProductionArg; j++)
				//{
				//	await sequencer.TestPointAsync(Steps.Notify);
				//}

				var updateTimerBeginCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerBegin);
				updateTimerBeginCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.IsActive, updateTimerBeginCycle.ProductionArg);

				var updateTimerEndCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.TryUpdateTimerEnd);
				updateTimerEndCycle.Complete();
				Assert.AreEqual(TickGeneratorTimerStatus.Changed, updateTimerEndCycle.ProductionArg);

				var endActiveProcessingCycle = await sequencer.TestPointAsync(TickGeneratorSequencer.EndActiveProcessing);
				endActiveProcessingCycle.Complete();
				Assert.AreEqual(TickGeneratorStatus.Operating, endActiveProcessingCycle.ProductionArg);
			}


			t = await tt;
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}


		[Test]
		public async Task DisposeBasicTest()
		{
			var tickGenerator = new TickGenerator(10);

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())
			;

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			tickGenerator.Dispose();
			Task t = await tickGenerator.Disposed.WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.ThrowsAsync<ObjectDisposedException>(async () => t = await tickGenerator.RegisterAsync(action).WrapIntoTask());
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			Assert.ThrowsAsync<ObjectDisposedException>(async () => t = await tickGenerator.UnRegisterAsync(action).WrapIntoTask());
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task MultipleRegistrationsTest()
		{
			var tickGenerator = new TickGenerator(10);

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())
			;

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			Task t = await tickGenerator.RegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			t = await tickGenerator.RegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			for (int i = 0; i < 10; i++)
			{
				await sequencer.TestPointAsync(Steps.Notify);
			}

			t = await tickGenerator.UnRegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}

		[Test]
		public async Task MultipleUnRegistrationsTest()
		{
			var tickGenerator = new TickGenerator(10);

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Steps.Notify, new StrategyOneOnOneUC())
			;

			Action action = () =>
			{
				sequencer.Point(SeqPointTypeUC.Notify, Steps.Notify);
			};

			Task t = await tickGenerator.RegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			for (int i = 0; i < 10; i++)
			{
				await sequencer.TestPointAsync(Steps.Notify);
			}

			t = await tickGenerator.UnRegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			t = await tickGenerator.UnRegisterAsync(action).WrapIntoTask();
			Assert.IsTrue(t.IsCompleted);
			Assert.IsFalse(t.IsCanceled);
			Assert.IsFalse(t.IsFaulted);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}
	}
}
