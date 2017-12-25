using System;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing.Test
{
	public partial class ISequencerUCTest
	{
		private static void ThrowWorker_ConditionalException(bool detect, ISequencerUC sequencer)
		{
			try
			{
				throw new Exception("Detect raised exception");
			}
			catch (Exception ex)
			{
				sequencer.Throw(() => detect, ex);
			}
		}

		[Test]
		public async Task ThrowTest_ConditionalException_ShouldDetect()
		{
			ISequencerUC sequencer;

			await
			SequencerUC
			.Construct()
			.Run(xsequencer => ThrowWorker_ConditionalException(true, xsequencer))
			.AssignOut(out sequencer)
			.WhenAll()
			;

			Assert.Throws<AggregateException>(() => sequencer.TryReThrowException());
		}


		[Test]
		public async Task ThrowTest_ConditionalException_ShouldNotDetect()
		{
			ISequencerUC sequencer;

			await SequencerUC
			.Construct()
			.Run(xsequencer => ThrowWorker_ConditionalException(false,xsequencer))
			.AssignOut(out sequencer)
			.WhenAll()
			;

			sequencer.TryReThrowException();
		}
	}
}
