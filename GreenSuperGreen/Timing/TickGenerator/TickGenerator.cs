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
		Task<int?> PeriodAsync { get; }
		Task<bool> Disposed { get; }
		Task RegisterAsync(Action asyncCallback);
		Task UnRegisterAsync(Action asyncCallback);

	}

	public class TickGenerator : ITickGenerator, IDisposable
	{
		private enum Disposal { None = 0, Disposing = 1, Disposed = 2 }
		private ISimpleLockUC Lock { get; } = new SpinLockUC();
		private IConcurrencyLevelCounter ConcurrencyLimiter { get; } = new ConcurrencyLevelLimiter(maxConcurrency: 1);

		public Task<int?> PeriodAsync { get; private set; }

		public Task<bool> Disposed => GetDisposedInfo();
		private Task<bool> GetDisposedInfo()
		{
			using (Lock.Enter())
			{
				return Status == Disposal.None ? TaskConstants.TaskFalse : TaskConstants.TaskTrue;
			}
		}

		private Disposal Status { get; set; } = Disposal.None;
		private Timer Timer { get; }
		private bool IsActive { get; set; }

		private ISet<Action> Actions { get; } = new HashSet<Action>();
		private IDictionary<Action, TaskCompletionSource<object>> AddActions { get; } = new Dictionary<Action, TaskCompletionSource<object>>();
		private IDictionary<Action, TaskCompletionSource<object>> RemoveActions { get; } = new Dictionary<Action, TaskCompletionSource<object>>();

		public TickGenerator(int period)
		{
			PeriodAsync = Task.FromResult<int?>(period < 1 ? 1 : period);
			Timer = new Timer(StaticCallBack, this, Timeout.Infinite, Timeout.Infinite);
		}

		private void TryUpdateTimer(bool activate)
		{
 			if (activate && !IsActive && PeriodAsync.Result.HasValue) { Timer.Change(PeriodAsync.Result.Value, PeriodAsync.Result.Value); IsActive = true; }
			if (!activate && IsActive) { Timer.Change(Timeout.Infinite, Timeout.Infinite); IsActive = false; }
		}

		private static void StaticCallBack(object obj) => (obj as TickGenerator)?.CallBack();

		private void CallBack()
		{
			using (var entry = ConcurrencyLimiter.TryEnter())
			{
				if (entry.HasEntry) CallBackExclusive();
			}
		}

		private void CallBackExclusive()
		{
			Disposal status;

			using (Lock.Enter())
			{
				status = Status;

				if (AddActions.Count > 0)
				{
					foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in AddActions)
					{
						Actions.Add(kvp.Key);
						kvp.Value.TrySetResult(null);
					}
				}
				if (RemoveActions.Count > 0)
				{
					foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in RemoveActions)
					{
						Actions.Remove(kvp.Key);
						kvp.Value.TrySetResult(null);
					}
				}
			}

			if (Actions.Count > 0 && status == Disposal.None)
			{
				foreach (Action action in Actions)
				{
					action();
				}
			}

			using (Lock.Enter())
			{
				if (Status == Disposal.None) TryUpdateTimer(activate: Actions.Count > 0);
				else
				{
					TryUpdateTimer(activate: false);
					PeriodAsync = TaskConstants.NullInt;
					Actions.Clear();
					ClearDictionary(AddActions, SetResult.Canceled);
					ClearDictionary(RemoveActions, SetResult.Completed);
					Status = Disposal.Disposed;
				}
			}
		}

		private enum SetResult { Completed, Canceled }

		private static void ClearDictionary(IDictionary<Action, TaskCompletionSource<object>> dictionary, SetResult result)
		{
			foreach (KeyValuePair<Action, TaskCompletionSource<object>> kvp in dictionary)
			{
				if (result == SetResult.Canceled) kvp.Value.TrySetCanceled();
				else if (result == SetResult.Completed) kvp.Value.TrySetResult(null);
			}
			dictionary.Clear();
		}

		public void Dispose()
		{
			if (Status >= Disposal.Disposing) return;
			using (Lock.Enter())
			{
				if (Status >= Disposal.Disposing) return;
				Status = Disposal.Disposing;
				TryUpdateTimer(activate: true);
			}
		}

		public Task RegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			if (Status >= Disposal.Disposing) return TaskConstants.TaskCanceled;
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			using (Lock.Enter())
			{
				if (Status >= Disposal.Disposing) return TaskConstants.TaskCanceled;
				AddActions.Add(asyncCallback, tcs);
				TryUpdateTimer(activate: true);
				return tcs.Task;
			}
		}

		public Task UnRegisterAsync(Action asyncCallback)
		{
			if (asyncCallback == null) throw new ArgumentNullException(nameof(asyncCallback));
			if (Status >= Disposal.Disposing) return TaskConstants.TaskCanceled;
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			using (Lock.Enter())
			{
				if (Status >= Disposal.Disposing) return TaskConstants.TaskCanceled;
				RemoveActions.Add(asyncCallback, tcs);
				TryUpdateTimer(activate: true);
				return tcs.Task;
			}
		}
	}
}
