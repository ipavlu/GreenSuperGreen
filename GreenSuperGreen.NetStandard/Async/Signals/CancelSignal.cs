using System;
using System.Threading;
using System.Threading.Tasks;

namespace GreenSuperGreen.Async.Signals
{
	public class CancelSignal : ACompletionUC<CancelSignal, CancelSignal>
	{
		public static implicit operator CancelSignal(CancellationTokenSource cts) => New(cts);
		public static implicit operator CancelSignal(CancellationToken cancellationToken) => New(cancellationToken);
		public static implicit operator CancelSignal(Task task) => New(task);
		public static implicit operator bool(CancelSignal ce) => ce?.IsRequested ?? false;
		public static implicit operator CancellationToken(CancelSignal ce) => ce?.CTSOutput?.Token ?? CancellationToken.None;
		public static explicit operator Task(CancelSignal ce) => ce?.Task ?? Task.CompletedTask;

		private Task TaskInput { get; }
		private CancellationToken CancellationTokenInput { get; }

		public Task Task => TaskTCS;

		private CancellationTokenSource CTSOutput { get; } = new CancellationTokenSource();
		public CancellationToken Token => CTSOutput.Token;

		public static CancelSignal New() => new CancelSignal();

		public bool IsRequested => (CTSOutput?.Token ?? CancellationToken.None).IsCancellationRequested;

		public CancelSignal()
		{
			Task.Run(EventProcessorAsync);
		}

		private async Task EventProcessorAsync()
		{
			try
			{
				await Task;
			}
			catch
			{
			}

			try
			{
				CTSOutput.Cancel();
			}
			catch (Exception)
			{
			}
		}

		public static CancelSignal New(CancelSignal cs) => new CancelSignal(cs);
		public CancelSignal(CancelSignal cs) : this(cs.Task)
		{
		}


		public static CancelSignal New(Task taskInput) => new CancelSignal(taskInput);
		public CancelSignal(Task taskInput) : this()
		{
			if ((TaskInput = taskInput) == null) return;
			Task.Run(TaskInputCancelled);
		}

		private async Task TaskInputCancelled()
		{
			try
			{
				Task task = TaskInput == null ? await Task.WhenAny(Task) : await Task.WhenAny(Task, TaskInput);
				if (task == Task) return;
			}
			catch (Exception ex)
			{
				SetException(ex);
				return;
			}

			TrySetCancelSafe();
		}

		public static CancelSignal New(CancellationTokenSource cts)
			=> new CancelSignal(cts)
			;
		public CancelSignal(CancellationTokenSource cts) : this(cts?.Token ?? CancellationToken.None) { }

		public static CancelSignal New(CancellationToken cancellationTokenInput)
			=> new CancelSignal(cancellationTokenInput)
			;
		public CancelSignal(CancellationToken cancellationTokenInput) : this()
		{
			CancellationTokenInput = cancellationTokenInput;
			if (!cancellationTokenInput.CanBeCanceled) return;
			cancellationTokenInput.Register(CancellationTokenInputCancelled);
		}

		private void CancellationTokenInputCancelled() => TrySetCancelSafe(CancellationTokenInput);

		public bool TrySetCancelSafe()
		{
			try
			{
				return SetCancellation();
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool TrySetCancelSafe(CancellationToken cancellationToken)
		{
			try
			{
				return SetCancellation(cancellationToken);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void Register(Action action)
		{
			if (action == null) return;
			CTSOutput.Token.Register(action);
		}

		public override string ToString()
			=> $"[{nameof(CancelSignal)}][{(IsRequested ? nameof(IsRequested) : "IsWaiting")}]"
			;
	}
}