using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	[TestFixture]
	public class MonitorLockThreadAbortResearch
	{
		private enum Worker
		{
			BeforeLock,
			Locked,
			Stuck
		}

		private void LockedWorker(ISequencerUC sequencer, object objLock)
		{
			try
			{
				sequencer.Point(SeqPointTypeUC.Notify, Worker.BeforeLock, Thread.CurrentThread);
				lock (objLock)
				{
					sequencer.Point(SeqPointTypeUC.Notify, Worker.Locked, Thread.CurrentThread);
					NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.Out);
					sequencer.Point(SeqPointTypeUC.Notify, Worker.Stuck, Thread.CurrentThread);
					pipeServer.WaitForConnection();
					pipeServer.Dispose();
					sequencer.Throw(null, $"It was supposed to stuck {nameof(NamedPipeServerStream)}.{nameof(NamedPipeServerStream.WaitForConnection)}in unmanaged part of code, but did not happened!");
				}
			}
			catch (Exception ex)
			{
				sequencer.Throw(ex, $"It was supposed to stuck {nameof(NamedPipeServerStream)}.{nameof(NamedPipeServerStream.WaitForConnection)}in unmanaged part of code, but did not happened!");
			}
			finally
			{
				sequencer.Throw(null, $"It was supposed to stuck {nameof(NamedPipeServerStream)}.{nameof(NamedPipeServerStream.WaitForConnection)}in unmanaged part of code, but did not happened!");
			}
		}

		[Ignore("ThreadAbortResearch: KEEP IT IGNORED!")]
		[Test]
		public async Task ThreadAbortDuringUnmanagedCall()
		{
			object objLock = new object();

			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Worker.BeforeLock, new StrategyOneOnOneUC())
			.Register(Worker.Locked, new StrategyOneOnOneUC())
			.Register(Worker.Stuck, new StrategyOneOnOneUC())
			;

			sequencer.Run(seq => LockedWorker(seq, objLock));//run 1st thread
			sequencer.Run(seq => LockedWorker(seq, objLock));//run 2nd thread
			sequencer.Run(seq => LockedWorker(seq, objLock));//run 3rd thread

			var test1 = await sequencer.TestPointAsync(Worker.BeforeLock);
			var test2 = await sequencer.TestPointAsync(Worker.BeforeLock);
			var test3 = await sequencer.TestPointAsync(Worker.BeforeLock);

			var th1 = test1.ProductionArg as Thread;
			var th2 = test2.ProductionArg as Thread;
			var th3 = test3.ProductionArg as Thread;
			//only one gets inside lock
			await sequencer.TestPointAsync(Worker.Locked);
			//and will stuck intentionally
			var intentionallyStuck = await sequencer.TestPointAsync(Worker.Stuck);

			await Task.Delay(100);
			await Task.Delay(100);
			await Task.Delay(100);
			await Task.Delay(100);
			await Task.Delay(100);
			await Task.Delay(100);

			try
			{
				Thread thread = intentionallyStuck.ProductionArg as Thread;
				await Task.Delay(100);
				thread?.Abort();
				th1?.Abort();
				th2?.Abort();
				th3?.Abort();
				while (true)
				{
					try
					{
						await Task.Delay(100);
						sequencer.TryReThrowException();
					}
					catch (Exception ex)
					{
						Assert.Warn(ex.Message);
					}
				}

			}
			catch (Exception ex)
			{
				Assert.Warn(ex.Message);
				throw;
			}
		}
	}
}

