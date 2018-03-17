using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenSuperGreen.Async;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantJumpStatement

namespace GreenSuperGreen.Timing
{
	public struct TimerProcessorCallBackRequest
	{
		public TimerProcessorRequest Request { get; }
		public TimerProcessorItem TimerProcessorItem { get; }

		public object TCS { get; }
		public ITaskCompletionSourceAccessor AccessorTCS { get; }

		public Task<TaskCompletionSource<TArg>> GetWrappedTCS<TArg>() => AccessorTCS?.GetTask(TCS) as Task<TaskCompletionSource<TArg>>;

		public AsyncTimerProcessorResult<TArg> GetAsyncResult<TArg>()
		{
			return new AsyncTimerProcessorResult<TArg>((TaskCompletionSource<TArg>)TimerProcessorItem.TimingTCS, GetWrappedTCS<TArg>());
		}

		public Task Task => AccessorTCS?.GetTask(TCS);

		public TimerProcessorCallBackRequest TrySetResult() { AccessorTCS.TrySetResult(TCS, TimerProcessorItem.TimingTCS); return this; }
		public TimerProcessorCallBackRequest TrySetCanceled() { AccessorTCS.TrySetCanceled(TCS); return this; }
		public TimerProcessorCallBackRequest TrySetDisposed() { AccessorTCS.TrySetException(TCS, TimerProcessor.DisposedException); return this; }
		public TimerProcessorCallBackRequest TrySetException(Exception e) { AccessorTCS.TrySetException(TCS, e); return this; }
		public TimerProcessorCallBackRequest TrySetException(string msg) => TrySetException(new Exception(msg));

		public static TimerProcessorCallBackRequest Add<TArg>(DateTime Now, TimeSpan Delay)
		=> new TimerProcessorCallBackRequest(	TimerProcessorRequest.Add,
												TimerProcessorItem.Add<TArg>(Now, Delay),
												new TaskCompletionSource<TaskCompletionSource<TArg>>(TaskCreationOptions.RunContinuationsAsynchronously),
												TaskCompletionSourceAccessor<TaskCompletionSource<TArg>>.Default)
		;

		public static TimerProcessorCallBackRequest AddWithResult<TArg>(DateTime Now, TimeSpan Delay, TArg result)
		=> new TimerProcessorCallBackRequest(	TimerProcessorRequest.Add,
												TimerProcessorItem.AddWithResult(Now, Delay, result),
												new TaskCompletionSource<TaskCompletionSource<TArg>>(TaskCreationOptions.RunContinuationsAsynchronously),
												TaskCompletionSourceAccessor<TaskCompletionSource<TArg>>.Default)
		;

		public static TimerProcessorCallBackRequest Remove<TArg>(TaskCompletionSource<TArg> tcs)
			=> new TimerProcessorCallBackRequest(	TimerProcessorRequest.Remove,
													TimerProcessorItem.Remove(tcs),
													new TaskCompletionSource<TaskCompletionSource<TArg>>(TaskCreationOptions.RunContinuationsAsynchronously),
													TaskCompletionSourceAccessor<TaskCompletionSource<TArg>>.Default)
		;

		private TimerProcessorCallBackRequest(	TimerProcessorRequest Request,
												TimerProcessorItem TimerProcessorItem,
												object TCS,
												ITaskCompletionSourceAccessor AccessorTCS)
		{
			this.Request = Request;
			this.TimerProcessorItem = TimerProcessorItem;
			this.TCS = TCS;
			this.AccessorTCS = AccessorTCS;
		}

		private void ProcessingAdd(TimerProcessorStatus status, IOrderedExpiryItems expiryItems)
		{
			if (status == TimerProcessorStatus.Operating)
			{
				if (expiryItems.TryAdd(TimerProcessorItem)) TrySetResult();
				else TrySetException($"{nameof(ProcessingAdd)}.TryAdd has failed");
				return;
			}
			if (status == TimerProcessorStatus.Disposing)
			{
				TrySetCanceled();
				return;
			}
			if (status == TimerProcessorStatus.Disposed)
			{
				TrySetDisposed();
			}
		}

		private void ProcessingRemove(TimerProcessorStatus status, IOrderedExpiryItems expiryItems)
		{
			if (status == TimerProcessorStatus.Operating)
			{
				expiryItems.TryRemove(TimerProcessorItem);
				TrySetResult();
				return;
			}
			if (status == TimerProcessorStatus.Disposing)
			{
				expiryItems.TryRemove(TimerProcessorItem);
				TrySetCanceled();
				return;
			}
			if (status == TimerProcessorStatus.Disposed)
			{
				TrySetDisposed();
				return;
			}
		}

		public static void Processing(TimerProcessorStatus status, Queue<TimerProcessorCallBackRequest> queue, IOrderedExpiryItems expiryItems)
		{
			while (queue.Count > 0)
			{
				TimerProcessorCallBackRequest request = queue.Dequeue();
				if (request.Request == TimerProcessorRequest.Add) request.ProcessingAdd(status, expiryItems);
				else if (request.Request == TimerProcessorRequest.Remove) request.ProcessingRemove(status, expiryItems);
			}
		}
	}
}