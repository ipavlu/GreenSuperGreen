using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.Async;
using GreenSuperGreen.Contexts;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Sequencing
{
	public
	class
	ProductionPointUC
	:	ACompletionUC<ProductionPointUC>,
		IProductionPointUC,
		ICompletionUC
	{
		private Action<object> InjectContinuation { get; }

		private ExecutionContext ExecutionContext { get; }
		private SynchronizationContext SynchronizationContext { get; }
		private TaskScheduler TaskScheduler { get; }

		public SeqPointTypeUC SeqPointType { get; }

		private int _completed;
		public bool Completed => Interlocked.Add(ref _completed, 0) != 0;

		public object ProductionArg { get; }

		private ISequencerTaskRegister TaskRegister { get; }
		private ISequencerExceptionRegister ExceptionRegister { get; }

		public ProductionPointUC(	ISequencerTaskRegister taskRegister,
									ISequencerExceptionRegister exceptionRegister,
									SeqPointTypeUC seqPointType,
									object productionArg = null,
									Action<object> injectContinuation = null)
		{
			SeqPointType = seqPointType;
			ProductionArg = productionArg;
			TaskRegister = taskRegister;
			ExceptionRegister = exceptionRegister;

			if (SeqPointType == SeqPointTypeUC.Match)
			{
				InjectContinuation = injectContinuation;
				ExecutionContext = InjectContinuation != null ? ExecutionContext.Capture() : null;
				SynchronizationContext = InjectContinuation != null ? SynchronizationContext.Current : null;
				TaskScheduler = InjectContinuation != null && SynchronizationContext == null ? TaskScheduler.Current : null;
				return;
			}

			if (SeqPointType == SeqPointTypeUC.Notify) Complete();

			exceptionRegister.Token.Register(OnCancel);
		}

		private void OnCancel()
		{
			SetException(ExceptionRegister.TryGetException());
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

		public void Complete(	object testArg = null,
								SeqContinuationUC context = SeqContinuationUC.OnCapturedContext)
		{
			if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)//allow completion only once
			{
				if (SeqPointType == SeqPointTypeUC.Match)
				{
					PostContinuation(context, testArg); //handles SetCompletion();
					return;
				}
				SetCompletion();
				return;
			}
			Exception ex = new InvalidOperationException($"{nameof(ProductionPointUC)}: Calling {nameof(Complete)} or {nameof(Fail)} multiple times!");
			ExceptionRegister.RegisterException(ex);
			throw ex;
		}

		public void Fail(Exception exception = null)
		{

			if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)//allow completion only once
			{
				SetException(exception ?? new InvalidOperationException($"{nameof(ProductionPointUC)}.{nameof(Fail)}: Reason for Exception was not provided."));
				return;
			}
			Exception ex = new InvalidOperationException($"{nameof(ProductionPointUC)}: Calling {nameof(Complete)} or {nameof(Fail)} multiple times!");
			ExceptionRegister.RegisterException(ex);
			throw ex;
		}

		private void PostContinuation(SeqContinuationUC context, object testArg)
		{
			Task t;
			if (context == SeqContinuationUC.OnCapturedContext)
			{
				if (SynchronizationContext != null)
				{
					SynchronizationContext.OperationStarted();
					SynchronizationContext.Post(ExecuteContinuation, testArg);
					return;
				}

				if (TaskScheduler != null)
				{
					t = Task.Factory.StartNew(ExecuteContinuation, testArg, CancellationToken.None, SafeStartNewOptions(), TaskScheduler);
					TaskRegister.RegisterTask(t);
					return;
				}

				Trace.WriteLine("Unknown synchronization context, defaulting to TaskScheduler.Default!");
				t = Task.Factory.StartNew(ExecuteContinuation, testArg, CancellationToken.None, SafeStartNewOptions(), TaskScheduler.Default);
				TaskRegister.RegisterTask(t);
				return;
			}

			if (context == SeqContinuationUC.OnNewContext)
			{
				t = Task.Factory.StartNew(ExecuteContinuation, testArg, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
				TaskRegister.RegisterTask(t);
				return;
			}

			if (context == SeqContinuationUC.OnCallersContext)
			{
				ExecuteContinuation(testArg);
			}

			throw new InvalidOperationException($"{nameof(PostContinuation)} did not captured any context in production code");
		}

		private void ExecuteContinuation(object testArg)
		{
			try
			{
				ExecutionContext.RunNullSafe(Continuation, testArg);
				SynchronizationContext?.OperationCompleted();
				SetCompletion();
			}
			catch (Exception ex)
			{
				ExceptionRegister.RegisterException(ex);
				SetException(ex);
			}
		}

		private void Continuation(object testArg) => InjectContinuation?.Invoke(testArg);
	}
}
