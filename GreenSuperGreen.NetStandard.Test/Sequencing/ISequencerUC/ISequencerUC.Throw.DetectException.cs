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
		private static void ThrowWorker_DetectException(ISequencerUC sequencer)
		{
			try
			{
				throw new Exception("Detect raised exception");
			}
			catch (Exception ex)
			{
				sequencer.Throw(ex);
			}
		}

		[Test]
		public async Task ThrowTest_DetectException()
		{
			ISequencerUC sequencer;

			await
			SequencerUC
			.Construct()
			.AssignOut(out sequencer)
			.Run(xsequencer => ThrowWorker_DetectException(xsequencer))
			.WhenAll()
			;

			Assert.Throws<AggregateException>(() => sequencer.TryReThrowException());
		}
	}
}
