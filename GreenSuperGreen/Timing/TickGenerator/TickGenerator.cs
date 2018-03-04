using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing
{
	public interface ITickGenerator : IDisposable
	{
		int? Period { get; }
		Task Disposed { get; }
		Task RegisterAsync(Action asyncCallback);
		Task UnRegisterAsync(Action asyncCallback);
	}

	public class TickGenerator : ITickGenerator, IDisposable
	{
		private enum StatusEnum { Operating = 0, Disposing = 1, Disposed = 2 }
		private ISimpleLockUC Lock { get; } = new SpinLockUC();
		private IConcurrencyLevelCounter ConcurrencyLimiter { get; } = new ConcurrencyLevelLimiter(maxConcurrency: 1);

		public int? Period { get; private set; }

		private TaskCompletionSource<object> DisposedTCS { get; } = new TaskCompletionSource<object>();
		public Task Disposed => DisposedTCS.Task;

		private StatusEnum Status { get; set; } = StatusEnum.Operating;
		private Timer Timer { get; }
		private bool IsActive { get; set; }

		private ISet<Action> Actions { get; } = new HashSet<Action>();
		private IDictionary<Action, TaskCompletionSource<object>> AddActions { get; } = new Dictionary<Action, TaskCompletionSource<object>>();
		private IDictionary<Action, TaskCompletionSource<object>> RemoveActions { get; } = new Dictionary<Action, TaskCompletionSource<object>>();

		public TickGenerator(int period)
		{
			Period = period < 1 ? 1 : period;
			Timer = new Timer(StaticCallBack, this, Timeout.Infinite, Timeout.Infinite);
		}

		private void TryUpdateTimer(bool activate)
		{
 			if (activate && !IsActive && Period.HasValue) { Timer.Change(Period.Value, Period.Value); IsActive = true; }
			if (!activate && IsActive) { Timer.Change(Timeout.Infinite, Timeout.Infinite); IsActive = false; }
		}

		private static void StaticCallBack(object obj) => (obj as TickGenerator)?.CallBack();

		private enum ProcessingResult { Completed, ExclusiveSkip }

		private void CallBack() => Processing(processActions: true);

		private ProcessingResult Processing(bool processActions)
		{
			using (var entry = ConcurrencyLimiter.TryEnter())
			{
				if (entry.HasEntry)
				{
					ProcessingExclusive(processActions);
					return ProcessingResult.Completed;
				}
				return ProcessingResult.ExclusiveSkip;
			}
		}

		private void ProcessingExclusive(bool processActions)
		{
			StatusEnum status;

			using (Lock.Enter())
			{
				if ((status = Status) != StatusEnum.Disposed)
				{
					ProcessAddActions();
					ProcessRemoveActions();
				}
			}

			if (processActions && status != StatusEnum.Disposed) ProcessActions();

			using (Lock.Enter())
			{
				if (Status == StatusEnum.Operating)
				{
					bool activate = false;
					activate |= Actions.Count > 0;
					activate |= AddActions.Count > 0;
					activate |= RemoveActions.Count > 0;
					TryUpdateTimer(activate);
					return;
				}

				ProcessDispose();
			}
		}

		private void ProcessDispose()
		{
			TryUpdateTimer(activate: false);
			Period = null;
			ClearAddActions();
			ClearRemoveActions();
			Actions.Clear();
			Status = StatusEnum.Disposed;
			DisposedTCS.TrySetResult(null);
		}

		private void ProcessAddActions()
		{
			if (AddActions.Count > 0)
			{
				foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in AddActions)
				{
					if (Actions.Add(kvp.Key)) kvp.Value.TrySetResult(null);
					else kvp.Value.TrySetCanceled();
				}
			}
		}

		private void ProcessRemoveActions()
		{
			if (RemoveActions.Count > 0)
			{
				foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in RemoveActions)
				{
					if (Actions.Remove(kvp.Key)) kvp.Value.TrySetResult(null);
					else kvp.Value.TrySetCanceled();
				}
			}
		}

		private void ProcessActions()
		{
			if (Actions.Count > 0)
			{
				foreach (Action action in Actions)
				{
					action();
				}
			}
		}

		private void ClearAddActions()
		{
			if (AddActions.Count > 0)
			{
				foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in AddActions)
				{
					kvp.Value.TrySetCanceled();
				}
			}
		}

		private void ClearRemoveActions()
		{
			if (RemoveActions.Count > 0)
			{
				foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in RemoveActions)
				{
					if (Actions.Remove(kvp.Key)) kvp.Value.TrySetResult(null);
					else kvp.Value.TrySetCanceled();
				}
			}
		}

		public void Dispose()
		{
			if (Status >= StatusEnum.Disposing) return;

			using (Lock.Enter())
			{
				if (Status >= StatusEnum.Disposing) return;
				Status = StatusEnum.Disposing;
			}

			for (int i = 0; i < 5; i++)
			{
				if (Processing(processActions: false) == ProcessingResult.Completed) return;
			}

			using (Lock.Enter())
			{
				if (Status == StatusEnum.Disposed) return;
				TryUpdateTimer(activate: true);
			}
		}

		public Task RegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			if (Status >= StatusEnum.Disposing) return TaskConstants.TaskCanceled;
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			using (Lock.Enter())
			{
				if (Status >= StatusEnum.Disposing) return TaskConstants.TaskCanceled;
				AddActions.Add(asyncCallback, tcs);
				TryUpdateTimer(activate: true);
				return tcs.Task;
			}
		}

		public Task UnRegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			if (Status >= StatusEnum.Disposing) return TaskConstants.TaskCanceled;
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			using (Lock.Enter())
			{
				if (Status >= StatusEnum.Disposing) return TaskConstants.TaskCanceled;
				RemoveActions.Add(asyncCallback, tcs);
				TryUpdateTimer(activate: true);
				return tcs.Task;
			}
		}
	}
}
