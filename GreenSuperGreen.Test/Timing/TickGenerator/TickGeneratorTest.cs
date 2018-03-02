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
		public async Task Test()
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
	}
}
