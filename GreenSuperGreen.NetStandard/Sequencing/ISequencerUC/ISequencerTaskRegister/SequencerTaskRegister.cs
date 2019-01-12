using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal class SequencerTaskRegister : ISequencerTaskRegister
	{
		private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();

		private ISequencerExceptionRegister ExceptionRegister { get; }

		public ISequencerUC SequencerUC { get; }

		public SequencerTaskRegister(ISequencerUC sequencerUC, ISequencerExceptionRegister exceptionRegister)
		{
			SequencerUC = sequencerUC;
			ExceptionRegister = exceptionRegister;
		}

		private void SetContinuation(Task t)
		{
			TaskContinuationOptions continuationOptions = TaskContinuationOptions.None;
			continuationOptions |= TaskContinuationOptions.DenyChildAttach;
			continuationOptions |= TaskContinuationOptions.HideScheduler;
			continuationOptions |= TaskContinuationOptions.RunContinuationsAsynchronously;

			t.ContinueWith(	Continuation,
							CancellationToken.None,
							continuationOptions,
							TaskScheduler.Default)
			;
		}

		private void Continuation(Task t)
		{
			if (t.IsFaulted)
			{
				ExceptionRegister?.RegisterException(t.Exception);
			}
		}

		public Task WhenAll() => Task.WhenAll(_tasks);

		public void RegisterTask(Task task)
		{
			SetContinuation(task);
			_tasks.Enqueue(task);
		}


		public void Run(Action<ISequencerUC> action)
		=> Run(() => action(SequencerUC))
		;

		public void Run(Action action)
		{
			Task t = Task.Factory.StartNew(action, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default);
			SetContinuation(t);
			_tasks.Enqueue(t);
		}

		private static TaskCreationOptions SafeStartNewOptions()
		{
			TaskCreationOptions options = TaskCreationOptions.None;
			options |= TaskCreationOptions.DenyChildAttach;
			options |= TaskCreationOptions.HideScheduler;
			options |= TaskCreationOptions.PreferFairness;
			options |= TaskCreationOptions.RunContinuationsAsynchronously;
			return options;
		}

		public void Run(object obj, Action<ISequencerUC,object> action)
		=> Run(obj, xObj => action(SequencerUC, xObj))
		;

		public void Run(object obj, Action<object> action)
		{
			Task t = Task.Factory.StartNew(action, obj, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default);
			SetContinuation(t);
			_tasks.Enqueue(t);
		}

		public void Run<TParameter>(TParameter parameter, Action<ISequencerUC, TParameter> action)
		{
			Task t = Task.Factory.StartNew(() => action(SequencerUC, parameter), CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default);
			SetContinuation(t);
			_tasks.Enqueue(t);
		}

		public void Run<TResult>(Func<ISequencerUC, TResult> func)
		=> Run(() => func(SequencerUC))
		;

		public void Run<TResult>(Func<TResult> func)
		{
			Task t = Task.Factory.StartNew(func, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default);
			SetContinuation(t);
			_tasks.Enqueue(t);
		}

		public void RunAsync(Func<ISequencerUC, Task> func)
		=> RunAsync(() => func(SequencerUC))
		;

		public void RunAsync(Func<Task> func)
		{
			Task t = Task.Factory.StartNew(func, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default).Unwrap();
			SetContinuation(t);
			_tasks.Enqueue(t);
		}

		public void RunAsync<TResult>(Func<ISequencerUC, Task<TResult>> func)
		=> RunAsync(() => func(SequencerUC))
		;

		public void RunAsync<TResult>(Func<Task<TResult>> func)
		{
			Task t = Task.Factory.StartNew(func, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default).Unwrap();
			SetContinuation(t);
			_tasks.Enqueue(t);
		}
	}
}