using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable RedundantJumpStatement
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing
{
	public enum TickGeneratorSequencer
	{
		TryUpdateTimerBegin,
		TryUpdateTimerEnd,

		CallBackProcessing,
		Processing,
		ExclusiveProcessing,
		BeginActiveProcessing,
		ActionsProcessing,
		ActionsProcessingCount,
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

	public enum TickGeneratorStatus { Operating = 0, Disposing = 1, Disposed = 2 }
	public enum TickGeneratorRequest { Add, Remove }

	[Flags]
	public enum TickGeneratorTimerStatus
	{
		None = (0 << 0),
		Activate = (1 << 1),
		IsActive = (1 << 2),
		Changed = (1 << 3)
	}


	public struct CallBackRequest
	{
		public static Exception DisposedException { get; } = new ObjectDisposedException($"{nameof(TickGenerator)} instance is disposed");
		public Action AsyncCallback { get; }
		public TickGeneratorRequest Request { get; }
		private TaskCompletionSource<object> TCS { get; }
		public Task Task => TCS.Task;
		public CallBackRequest TrySetResult() { TCS.TrySetResult(null); return this; }
		public CallBackRequest TrySetCanceled() { TCS.TrySetCanceled(); return this; }
		public CallBackRequest TrySetDisposed() { TCS.TrySetException(DisposedException); return this; }

		public CallBackRequest(Action asyncCallback, TickGeneratorRequest request)
		{
			TCS = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			AsyncCallback = asyncCallback;
			Request = request;
		}

		private void ProcessingAdd(TickGeneratorStatus status, ISet<Action> set)
		{
			if (status == TickGeneratorStatus.Operating)
			{
				set.Add(AsyncCallback);
				TrySetResult();
				return;
			}
			if (status == TickGeneratorStatus.Disposing)
			{
				TrySetCanceled();
				return;
			}
			if (status == TickGeneratorStatus.Disposed)
			{
				TrySetDisposed();
			}
		}

		private void ProcessingRemove(TickGeneratorStatus status, ISet<Action> set)
		{
			if (status == TickGeneratorStatus.Operating)
			{
				set.Remove(AsyncCallback);
				TrySetResult();
				return;
			}
			if (status == TickGeneratorStatus.Disposing)
			{
				set.Remove(AsyncCallback);
				TrySetResult();
				return;
			}
			if (status == TickGeneratorStatus.Disposed)
			{
				set.Remove(AsyncCallback);
				TrySetDisposed();
				return;
			}
		}

		public static void Processing(TickGeneratorStatus status, Queue<CallBackRequest> queue, ISet<Action> set)
		{
			while (queue.Count > 0)
			{
				CallBackRequest request = queue.Dequeue();
				if (request.Request == TickGeneratorRequest.Add) request.ProcessingAdd(status, set);
				else if (request.Request == TickGeneratorRequest.Remove) request.ProcessingRemove(status, set);
			}
		}
	}


	public interface ITickGenerator : IDisposable
	{
		int? Period { get; }
		Task Disposed { get; }
		Task RegisterAsync(Action asyncCallback);
		Task UnRegisterAsync(Action asyncCallback);
	}

	public class TickGenerator : ITickGenerator, IDisposable
	{
		private ISequencerUC NullSafeSequencer { get; }
		private ISimpleLockUC Lock { get; } = new SpinLockUC();
		private IConcurrencyLevelCounter ConcurrencyLimiter { get; } = new ConcurrencyLevelLimiter(maxConcurrency: 1);

		public int? Period { get; private set; }

		private TaskCompletionSource<object> DisposedTCS { get; } = new TaskCompletionSource<object>();
		public Task Disposed => DisposedTCS.Task;

		private TickGeneratorStatus Status { get; set; } = TickGeneratorStatus.Operating;
		private Timer Timer { get; }
		private bool IsActive { get; set; }
		private bool ActiveProcessing { get; set; }

		private ISet<Action> Actions { get; } = new HashSet<Action>();
		private Queue<CallBackRequest> Requests { get; } = new Queue<CallBackRequest>();
		private static int MaxActiveRequests { get; } = Environment.ProcessorCount;
		private Queue<CallBackRequest> WorkingRequests { get; } = new Queue<CallBackRequest>(MaxActiveRequests);

		public TickGenerator(int period, ISequencerUC sequencer = null)
		{
			NullSafeSequencer = sequencer;
			Period = period < 1 ? 1 : period;
			Timer = new Timer(StaticCallBack, this, Timeout.Infinite, Timeout.Infinite);
		}
		
		protected virtual TickGeneratorTimerStatus TryUpdateTimer(bool activate)
		{
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.TryUpdateTimerBegin, IsActive ? TickGeneratorTimerStatus.IsActive : TickGeneratorTimerStatus.None);

			TickGeneratorTimerStatus result = TickGeneratorTimerStatus.None;
			result |= activate ? TickGeneratorTimerStatus.Activate : result;

			if (activate && !IsActive && Period.HasValue) { Timer.Change(Period.Value, Period.Value); IsActive = true; result |= TickGeneratorTimerStatus.Changed; }
			if (!activate && IsActive) { Timer.Change(Timeout.Infinite, Timeout.Infinite); IsActive = false; result |= TickGeneratorTimerStatus.Changed; }

			result |= IsActive ? TickGeneratorTimerStatus.IsActive : result;

			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.TryUpdateTimerEnd, result);
			return result;
		}

		public enum ProcessingResult { Processed, ExclusiveSkip }

		private static void StaticCallBack(object obj) => (obj as TickGenerator)?.CallBackProcessing();

		protected virtual void CallBackProcessing()
		{
			NullSafeSequencer.Point(SeqPointTypeUC.Match, TickGeneratorSequencer.CallBackProcessing);
			Processing(processActions: true);
		}

		protected virtual ProcessingResult Processing(bool processActions)
		{
			using (var entry = ConcurrencyLimiter.TryEnter())
			{
				ProcessingResult result = entry.HasEntry ? ProcessingResult.Processed : ProcessingResult.ExclusiveSkip;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.Processing, result);
				if (result == ProcessingResult.Processed) ExclusiveProcessing(processActions);
				return result;
			}
		}

		protected virtual void ExclusiveProcessing(bool processActions)
		{
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.ExclusiveProcessing, processActions);
			TickGeneratorStatus lastStatus;

			using (Lock.Enter())
			{
				ActiveProcessing = true;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.BeginActiveProcessing, Status);
			}

			while (true)
			{
				using (Lock.Enter())
				{
					lastStatus = Status;
					while (Requests.Count > 0 && WorkingRequests.Count < MaxActiveRequests) WorkingRequests.Enqueue(Requests.Dequeue());
					if (Requests.Count <= 0 && WorkingRequests.Count <= 0) break;
				}

				CallBackRequest.Processing(lastStatus, WorkingRequests, Actions);
			}

			bool processing = processActions && lastStatus != TickGeneratorStatus.Disposed && Actions.Count > 0;
			NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.ActionsProcessing, processing);

			if (processing)
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.ActionsProcessingCount, Actions.Count);
				foreach (Action action in Actions)
				{
					action();
				}
			}

			while (true)
			{
				using (Lock.Enter())
				{
					lastStatus = Status;
					while (Requests.Count > 0 && WorkingRequests.Count < MaxActiveRequests) WorkingRequests.Enqueue(Requests.Dequeue());
					if (Requests.Count <= 0 && WorkingRequests.Count <= 0) break;
				}

				CallBackRequest.Processing(lastStatus, WorkingRequests, Actions);
			}

			using (Lock.Enter())
			{
				ActiveProcessing = false;
				CallBackRequest.Processing(lastStatus, Requests, Actions);

				if (Status == TickGeneratorStatus.Operating)
				{
					bool activate = false;
					activate |= Actions.Count > 0;
					activate |= Requests.Count > 0;
					TryUpdateTimer(activate);
				}
				else
				{
					TryUpdateTimer(activate: false);
					Period = null;
					Actions.Clear();
					Status = TickGeneratorStatus.Disposed;
					DisposedTCS.TrySetResult(null);
				}
				NullSafeSequencer.PointArg(SeqPointTypeUC.Match, TickGeneratorSequencer.EndActiveProcessing, Status);
			}
		}

		public void Dispose()
		{
			if (Status >= TickGeneratorStatus.Disposing) return;

			using (Lock.Enter())
			{
				if (Status >= TickGeneratorStatus.Disposing) return;
				Status = TickGeneratorStatus.Disposing;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.DisposeStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.DisposeActiveProcessing, ActiveProcessing);
				if (ActiveProcessing) return;
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return;

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.DisposedEnd, Status);
				if (Status == TickGeneratorStatus.Disposed) return;
				TryUpdateTimer(activate: true);
			}
		}

		public Task RegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			CallBackRequest request = new CallBackRequest(asyncCallback, TickGeneratorRequest.Add);
			if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;
			if (Status == TickGeneratorStatus.Disposing) return request.TrySetCanceled().Task;

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.RegisterStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.RegisterActiveProcessing, ActiveProcessing);
				if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;
				if (Status == TickGeneratorStatus.Disposing) return request.TrySetCanceled().Task;
				Requests.Enqueue(request);
				if (ActiveProcessing) return request.Task;
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return request.Task;

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.RegisterEnd, Status);
				if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;
				TryUpdateTimer(activate: true);
				return request.Task;
			}
		}

		public Task UnRegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			CallBackRequest request = new CallBackRequest(asyncCallback, TickGeneratorRequest.Remove);
			if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;

			using (Lock.Enter())
			{
				if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.UnRegisterStatus, Status);
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.UnRegisterActiveProcessing, ActiveProcessing);
				Requests.Enqueue(request);
				if (ActiveProcessing) return request.Task;
			}

			if (Processing(processActions: false) == ProcessingResult.Processed) return request.Task;

			using (Lock.Enter())
			{
				NullSafeSequencer.PointArg(SeqPointTypeUC.Notify, TickGeneratorSequencer.UnRegisterEnd, Status);
				if (Status == TickGeneratorStatus.Disposed) return request.TrySetDisposed().Task;
				TryUpdateTimer(activate: true);
				return request.Task;
			}
		}
	}
}