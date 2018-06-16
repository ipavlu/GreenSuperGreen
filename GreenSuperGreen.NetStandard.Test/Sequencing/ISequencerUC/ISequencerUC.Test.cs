using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#pragma warning disable 4014//not awaited async

namespace GreenSuperGreen.Sequencing.Test
{
	[TestFixture]
	public partial class ISequencerUCTest
	{
		public static void Print([CallerMemberName] string Name = null)
		{
			string msg = string.Empty;
			msg += $"[{Name}]";
			msg += $"[Trhead:{Thread.CurrentThread.ManagedThreadId}]";

			ExecutionContext ec = ExecutionContext.Capture();
			msg += $"EC:{ec?.GetHashCode()}]";

			TaskScheduler ts = TaskScheduler.Current;
			msg += $"TS:{ts?.Id}]";

			SynchronizationContext sc = SynchronizationContext.Current;
			msg += $"SC:{sc?.GetHashCode()}]";

			Trace.WriteLine(msg);
		}

		private class ProductionCode
		{
			public ISequencerUC Sequencer { get; }

			public ProductionCode(ISequencerUC sequencer)
			{
				Sequencer = sequencer;
			}

			public enum SequencingEnumTest
			{
				SpotBegin,
				SpotEnd
			}

			public async Task Worker(string parameter)
			{
				await Sequencer.PointAsync(SeqPointTypeUC.Match, SequencingEnumTest.SpotBegin, parameter);

				//some operations with driven thread execution by unit test
				Trace.WriteLine(parameter);

				await Sequencer.PointAsync(SeqPointTypeUC.Match, SequencingEnumTest.SpotEnd, parameter);
			}
		}


		[Test]
		public async Task TestAB()
		{
			//create sequencer
			ISequencerUC sequencer = SequencerUC.Construct();

			//create production code
			ProductionCode worker = new ProductionCode(sequencer);

			//register production code sequencer event spots
			//strategy, one production code event is translated to one unit test code event
			sequencer
			.Register(ProductionCode.SequencingEnumTest.SpotBegin, new StrategyOneOnOneUC())
			.Register(ProductionCode.SequencingEnumTest.SpotEnd, new StrategyOneOnOneUC())
			;

			//start concurrent tasks
			Task.Run(() => { worker.Worker("A"); });
			Task.Run(() => { worker.Worker("B"); });


			//await two production code events
			IProductionPointUC taskWorkerBegin1 = await worker.Sequencer.TestPointAsync(ProductionCode.SequencingEnumTest.SpotBegin);
			IProductionPointUC taskWorkerBegin2 = await worker.Sequencer.TestPointAsync(ProductionCode.SequencingEnumTest.SpotBegin);

			IProductionPointUC taskWorkerBeginA;
			IProductionPointUC taskWorkerBeginB;

			//detect which event is which
			taskWorkerBeginA = (string)taskWorkerBegin1.ProductionArg == "A" ? taskWorkerBegin1 : null;
			taskWorkerBeginA = (string)taskWorkerBegin2.ProductionArg == "A" ? taskWorkerBegin2 : taskWorkerBeginA;

			//detect which event is which
			taskWorkerBeginB = (string)taskWorkerBegin1.ProductionArg == "B" ? taskWorkerBegin1 : null;
			taskWorkerBeginB = (string)taskWorkerBegin2.ProductionArg == "B" ? taskWorkerBegin2 : taskWorkerBeginB;

			//decide about the order of execution
			taskWorkerBeginA.Complete("A runs first");

			//await A to run to SpotB
			IProductionPointUC taskWorkerEndA = await worker.Sequencer.TestPointAsync(ProductionCode.SequencingEnumTest.SpotEnd);

			//decide about the order of execution
			taskWorkerBeginB.Complete("B runs second");


			IProductionPointUC taskWorkerEndB = await worker.Sequencer.TestPointAsync(ProductionCode.SequencingEnumTest.SpotEnd);
			taskWorkerEndA.Complete("A continue");
			taskWorkerEndB.Complete("B continue");
		}
	}
}
