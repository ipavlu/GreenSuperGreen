using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Async.Signals;


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace GreenSuperGreen.Async.Signals.Test
{
	[TestFixture]
	public class CancelSignalTest
	{
		[Test]
		public async Task DriveCancellatioWithWhenAny()
		{
			CancelSignal cs = CancelSignal.New();
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			cs.Token.Register(() => tcs.TrySetCanceled());
			

			//Task t = Task.Run(async () =>
			//{
			//	await Task.Delay(10000);
			//	cs.TrySetCancelSafe(); ;
			//});

			cs.TrySetCancelSafe(); ;


			Task delay = Task.Delay(20000);
			Task d = await Task.WhenAny(delay, cs.Task);
			await d;
			await Task.Delay(30000);
		}



		[Test]
		public async Task WhenAnyCancellation()
		{
			CancellationTokenSource cts = new CancellationTokenSource();


			CancelSignal cs = CancelSignal.New(cts.Token);

			bool aa = cs;

			if (cs)
			{
				//exit
			}

			CancelSignal cs2 = CancelSignal.New(cs);
			cs.TrySetCancelSafe();

			Task delay = Task.Delay(10000);

			Task t = await Task.WhenAny(delay, cs.Task);

			if (t == cs2.Task)
			{
				throw new Exception("cancel");
			}

			await Task.Delay(1000);
			await Task.Delay(1000);
			await Task.Delay(1000);
			await Task.Delay(1000);
			await Task.Delay(1000);

			//Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}


		[Test]
		public async Task MultiSource()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			Task task = tcs.Task;

			CancellationTokenSource cts = new CancellationTokenSource(3000);
			CancellationToken token = cts.Token;

			CancelSignal cs = CancelSignal.New(token);
			cs.TrySetCancelSafe();
			Task t0 = (Task)cs;
			Task t1 = cs.Task;
			CancellationToken cst0 = cs.Token;
			CancellationToken cst1 = cs;

			CancelSignal cs2 = CancelSignal.New(cs);

			using (MemoryStream ms = new MemoryStream())
			{
				Task delay = Task.Delay(20000);
				Task cancel = cs2.Task;
				Task work = ms.WriteAsync(new byte[] { 0 }, 0, 0, cs2);

				Task t = await Task.WhenAny(delay, cancel, work);
				if (t == work)
				{

				}
				else if (t == delay)
				{
				}
			}
		}


		[Test]
		public async Task TestTaskCanceledOnTrySetResult()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelSignal cs = CancelSignal.New(tcs.Task);
			Task task = cs.Task;
			_ = Task.Run(() => TrySetResult(tcs));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}

		private async Task TrySetResult(TaskCompletionSource<object> tcs)
		{
			tcs.TrySetResult(null);

		}

		[Test]
		public async Task TestTaskCanceledOnTrySetException()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelSignal cs = CancelSignal.New(tcs.Task);
			Task task = cs.Task;
			_ = Task.Run(() => TrySetException(tcs));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}

		private async Task TrySetException(TaskCompletionSource<object> tcs)
		{
			tcs.TrySetException(new Exception());
		}

		[Test]
		public async Task TestTaskCanceledOnTrySetCanceled()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelSignal cs = CancelSignal.New(tcs.Task);
			Task task = cs.Task;
			_ = Task.Run(() => TrySetCanceled(tcs));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}

		private async Task TrySetCanceled(TaskCompletionSource<object> tcs)
		{
			tcs.TrySetCanceled();
		}


		[Test]
		public async Task TestTaskCanceledOnTrySetCanceledCancel()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelSignal cs = CancelSignal.New(tcs.Task);
			Task task = cs.Task;
			_ = Task.Run(() => TrySetCanceled(tcs, CancellationToken.None));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}

		[Test]
		public async Task TestTaskCanceledOnTrySetCanceledTokenSelf()
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelSignal cs = CancelSignal.New(tcs.Task);
			Task task = cs.Task;
			_ = Task.Run(() => TrySetCanceled(tcs, cs.Token));
			Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
		}

		private async Task TrySetCanceled(TaskCompletionSource<object> tcs, CancellationToken ct)
		{
			tcs.TrySetCanceled(ct);
		}
	}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}