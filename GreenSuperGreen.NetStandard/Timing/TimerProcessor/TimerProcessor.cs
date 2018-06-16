using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable RedundantJumpStatement
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing
{
	public enum TimerProcessorSequencer
	{
		TryUpdateTimerBegin,
		TryUpdateTimerEnd,

		CallBackProcessing,
		Processing,
		ExclusiveProcessing,
		BeginActiveProcessing,
		ActionsProcessing,
		ActionsProcessingCount,
		ActionsProcessingExpired,
		EndActiveProcessing,

		DisposeStatus,
		DisposeActiveProcessing,
		DisposedEnd,

		RegisterStatus,
		RegisterActiveProcessing,
		RegisterEnd,

		UnRegisterStatus,
		UnRegisterActiveProcessing,
		UnRegisterEnd
	}

	public enum TimerProcessorStatus { Operating = 0, Disposing = 1, Disposed = 2 }
	public enum TimerProcessorRequest { Add, Remove }

	[Flags]
	public enum TimerProcessorTimerStatus
	{
		None = (0 << 0),
		Activate = (1 << 1),
		IsActive = (1 << 2),
		Changed = (1 << 3)
	}


	public interface ITimerProcessor : IDisposable
	{
		int? Period { get; }
		Task Disposed { get; }
		AsyncTimerProcessorResult<TArg> RegisterResultAsync<TArg>(TimeSpan delay, TArg result);
		AsyncTimerProcessorResult<TArg> RegisterAsync<TArg>(TimeSpan delay);
		AsyncTimerProcessorResult<TArg> UnRegisterAsync<TArg>(TaskCompletionSource<TArg> tcs);
	}

	/// <summary>
	/// TimerProcessor is resource lightweight attempt to handle large numbers of timing items.
	/// If no items are processed, then ticking is stopped and does not take CPU time, ThreadPool resources.
	/// Registration is lock free, spin-lock based. Each operation is made to limit spinwaiting to minimum.
	/// </summary>
	public class TimerProcessor : ITimerProcessor, IDisposable
	{
		public static TimeSpan MinDelay { get; } = TimeSpan.FromMilliseconds(1);
		public static Exception DisposedException => new ObjectDisposedException($"{nameof(TimerProcessor)} instance is disposed");

		private ISequencerUC NullSafeSequencer { get; }
		private ILockUC Lock { get; } = new SpinLockUC();
		private IConcurrencyLevelCounter ConcurrencyLimiter { get; } = new ConcurrencyLevelLimiter(maxConcurrency: 1);

		public int? Period => Ticker?.Period;

		private TaskCompletionSource<object> DisposedTCS { get; } = new TaskCompletionSource<object>();
		public Task Disposed => DisposedTCS.Task;

		private TimerProcessorStatus Status { get; set; } = TimerProcessorStatus.Operating;
		private TickGenerator Ticker { get; }
		private bool IsActive { get; set; }
		private bool ActiveProcessing { get; set; }
		private IRealTimeSource RealTimeSource { get; }

		private IOrderedExpiryItems ExpiryItems { get; } = new OrderedExpiryItems();
		private Queue<TimerProcessorCallBackRequest> Requests { get; } = new Queue<TimerProcessorCallBackRequest>();
		private static int MaxActiveRequests { get; } = Environment.ProcessorCount;
		private Queue<TimerProcessorCallBackRequest> WorkingRequests { get; } = new Queue<TimerProcessorCallBackRequest>(MaxActiveRequests);

		public TimerProcessor(int period, IRealTimeSource realTimeSource, ISequencerUC sequencer = null)
		{
			if ((RealTimeSource = realTimeSource) == null) throw new ArgumentNullException($"{nameof(realTimeSource)}");
			Ticker = new TickGenerator(period, NullSafeSequencer = sequencer);
		}

		protected virtual TimerProcessorTimerStatus TryUpdateTimer(bool activate)
		{
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.TryUpdateTimerBegin, IsActive ? TimerProcessorTimerStatus.IsActive : TimerProcessorTimerStatus.None);

			TimerProcessorTimerStatus result = TimerProcessorTimerStatus.None;
			result |= activate ? TimerProcessorTimerStatus.Activate : result;

			if (activate && !IsActive && Period.HasValue) { Ticker.RegisterAsync(CallBackProcessing); IsActive = true; result |= TimerProcessorTimerStatus.Changed; }
			if (!activate && IsActive) { Ticker.UnRegisterAsync(CallBackProcessing); IsActive = false; result |= TimerProcessorTimerStatus.Changed; }

			result |= IsActive ? TimerProcessorTimerStatus.IsActive : result;

			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.TryUpdateTimerEnd, result);
			return result;
		}

		public enum ProcessingResult { Processed, ExclusiveSkip }

		protected virtual void CallBackProcessing()
		{
			NullSafeSequencer.Point(SeqPointTypeUC.Match, TimerProcessorSequencer.CallBackProcessing);
			Processing(processActions: true);
		}

		protected virtual ProcessingResult Processing(bool processActions)
		{
			using (var entry = ConcurrencyLimiter.TryEnter())
			{
				ProcessingResult result = entry.HasEntry ? ProcessingResult.Processed : ProcessingResult.ExclusiveSkip;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.Processing, result);
				if (result == ProcessingResult.Processed) ExclusiveProcessing(processActions);
				return result;
			}
		}

		protected virtual void ExclusiveProcessing(bool processActions)
		{
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.ExclusiveProcessing, processActions);
			TimerProcessorStatus lastStatus;

			using (Lock.Enter())
			{
				ActiveProcessing = true;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.BeginActiveProcessing, Status);
			}

			while (true)
			{
				using (Lock.Enter())
				{
					lastStatus = Status;
					while (Requests.Count > 0 && WorkingRequests.Count < MaxActiveRequests) WorkingRequests.Enqueue(Requests.Dequeue());
					if (Requests.Count <= 0 && WorkingRequests.Count <= 0) break;
				}

				TimerProcessorCallBackRequest.Processing(lastStatus, WorkingRequests, ExpiryItems);
			}

			bool processing = processActions && lastStatus != TimerProcessorStatus.Disposed && ExpiryItems.Any();
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.ActionsProcessing, processing);

			if (processing)
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.ActionsProcessingCount, ExpiryItems.Count);
				ExpiryItems.TryExpire(RealTimeSource.GetUtcNow());
			}

			while (true)
			{
				using (Lock.Enter())
				{
					lastStatus = Status;
					while (Requests.Count > 0 && WorkingRequests.Count < MaxActiveRequests) WorkingRequests.Enqueue(Requests.Dequeue());
					if (Requests.Count <= 0 && WorkingRequests.Count <= 0) break;
				}

				TimerProcessorCallBackRequest.Processing(lastStatus, WorkingRequests, ExpiryItems);
			}

			using (Lock.Enter())
			{
				ActiveProcessing = false;
				TimerProcessorCallBackRequest.Processing(Status, Requests, ExpiryItems);

				if (Status == TimerProcessorStatus.Operating)
				{
					bool activate = false;
					activate |= ExpiryItems.Count > 0;
					activate |= Requests.Count > 0;
					TryUpdateTimer(activate);
				}
				else
				{
					TryUpdateTimer(activate: false);
					ExpiryItems.CancelAllAndClear();
					Status = TimerProcessorStatus.Disposed;
					Ticker.Dispose();
					DisposedTCS.TrySetResult(null);
				}
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TimerProcessorSequencer.EndActiveProcessing, Status);
			}
		}

		public void Dispose()
		{
			if (Status >= TimerProcessorStatus.Disposing) return;

			using (Lock.Enter())
			{
				if (Status >= TimerProcessorStatus.Disposing) return;
				Status = TimerProcessorStatus.Disposing;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.DisposeStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.DisposeActiveProcessing, ActiveProcessing);
				if (ActiveProcessing) return;
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return;

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.DisposedEnd, Status);
				if (Status == TimerProcessorStatus.Disposed) return;
				TryUpdateTimer(activate: true);
			}
		}

		public AsyncTimerProcessorResult<TArg> RegisterResultAsync<TArg>(TimeSpan delay, TArg result)
		{
			TimerProcessorCallBackRequest request = TimerProcessorCallBackRequest.AddWithResult(RealTimeSource.GetUtcNow(), delay, result);
			if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
			if (Status == TimerProcessorStatus.Disposing) return request.TrySetCanceled().GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterActiveProcessing, ActiveProcessing);
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				if (Status == TimerProcessorStatus.Disposing) return request.TrySetCanceled().GetAsyncResult<TArg>();
				Requests.Enqueue(request);
				if (ActiveProcessing) return request.GetAsyncResult<TArg>();
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return request.GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterEnd, Status);
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				TryUpdateTimer(activate: true);
				return request.GetAsyncResult<TArg>();
			}
		}

		public AsyncTimerProcessorResult<TArg> RegisterAsync<TArg>(TimeSpan delay)
		{
			TimerProcessorCallBackRequest request = TimerProcessorCallBackRequest.Add<TArg>(RealTimeSource.GetUtcNow(), delay);
			if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
			if (Status == TimerProcessorStatus.Disposing) return request.TrySetCanceled().GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterActiveProcessing, ActiveProcessing);
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				if (Status == TimerProcessorStatus.Disposing) return request.TrySetCanceled().GetAsyncResult<TArg>();
				Requests.Enqueue(request);
				if (ActiveProcessing) return request.GetAsyncResult<TArg>();
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return request.GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.RegisterEnd, Status);
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				TryUpdateTimer(activate: true);
				return request.GetAsyncResult<TArg>();
			}
		}

		public AsyncTimerProcessorResult<TArg> UnRegisterAsync<TArg>(TaskCompletionSource<TArg> tcs)
		{
			if (tcs == null) throw new ArgumentNullException(nameof(tcs));
			TimerProcessorCallBackRequest request = TimerProcessorCallBackRequest.Remove(tcs);
			if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.UnRegisterStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.UnRegisterActiveProcessing, ActiveProcessing);
				Requests.Enqueue(request);
				if (ActiveProcessing) return request.GetAsyncResult<TArg>();
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return request.GetAsyncResult<TArg>();

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TimerProcessorSequencer.UnRegisterEnd, Status);
				if (Status == TimerProcessorStatus.Disposed) return request.TrySetDisposed().GetAsyncResult<TArg>();
				TryUpdateTimer(activate: true);
				return request.GetAsyncResult<TArg>();
			}
		}
	}
}