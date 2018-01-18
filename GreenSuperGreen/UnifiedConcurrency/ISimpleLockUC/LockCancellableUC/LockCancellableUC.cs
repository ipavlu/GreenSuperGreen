//using System.Collections.Concurrent;
//using System.Runtime.CompilerServices;
//using System.Threading;
//using System.Threading.Tasks;

//// ReSharper disable CheckNamespace
//// ReSharper disable InconsistentNaming
//// ReSharper disable SuggestVarOrType_BuiltInTypes

//namespace GreenSuperGreen.UnifiedConcurrency
//{
//	/// <summary>
//	/// <para/> <see cref="SpinLockUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
//	/// <para/> Does not support recursive call and does not protect against recursive call!
//	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered...
//	/// </summary>
//	public class LockCancellableUC : ISimpleLockUC
//	{
//		private static Task CancellationNoneTask { get; } = Task.FromCanceled(CancellationToken.None);
//		private ISimpleLockUC Lock { get; } = new SpinLockUC();
//		private ConcurrentQueue<TaskCompletionSource<object>> Queue { get; } = new ConcurrentQueue<TaskCompletionSource<object>>();
//		private TaskCompletionSource<object>  _access;
//		private EntryCompletionUC EntryCompletion { get; }
//		private Task CancellationTask { get; } = CancellationNoneTask;

//		public SyncPrimitiveCapabilityUC Capability { get; } = 0
//		| SyncPrimitiveCapabilityUC.Enter
//		| SyncPrimitiveCapabilityUC.TryEnter
//		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
//		| SyncPrimitiveCapabilityUC.Cancellable
//		| SyncPrimitiveCapabilityUC.NonRecursive
//		| SyncPrimitiveCapabilityUC.NonThreadAffine
//		;

//		public LockCancellableUC()
//		{
//			EntryCompletion = new EntryCompletionUC(ExitAction);
//		}

//		public LockCancellableUC(CancellationToken cancellationToken)
//		{
//			CancellationTask = Task.FromCanceled(cancellationToken);
//			EntryCompletion = new EntryCompletionUC(ExitAction);
//		}

//		private void CheckCancellationRequested([CallerMemberName]string methodName = "")
//		{
//			if (!CancellationTask.IsCanceled) return;
//			throw new TaskCanceledException($"{nameof(LockUC)}:{methodName} - cancellation requested");
//		}

//		private void ExitAction() => Exit();

//		private void Exit([CallerMemberName]string methodName = "")
//		{
//			bool canceled;
//			using (Lock.Enter())
//			{
//				canceled = _access.Task.IsCanceled;
//				while (true)
//				{
//					if (!Queue.TryDequeue(out _access)) break;
//					if (_access?.TrySetResult(null) == true) break;
//					_access = null;
//				}
//			}
//			CheckCancellationRequested();
//			if (canceled) throw new TaskCanceledException($"{nameof(LockUC)}.{nameof(Exit)}:{methodName}:ThreadId:{Thread.CurrentThread.ManagedThreadId}");
//		}

//		private enum Status { HasEntry, WaitingForEntry}

//		public EntryBlockUC Enter()
//		{
//			CheckCancellationRequested();
//			TaskCompletionSource<object> access = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
//			Status status;

//			using (Lock.Enter())
//			{
//				if (_access == null)
//				{
//					_access = access;
//					status = Status.HasEntry;
//				}
//				else
//				{
//					Queue.Enqueue(access);
//					status = Status.WaitingForEntry;
//				}
//			}

//			if (status == Status.HasEntry) return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);

//			Task.WhenAny(CancellationTask, access.Task).Wait();
//			CheckCancellationRequested();
//			if (access.Task.IsCanceled) Exit();

//			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
//		}

//		public EntryBlockUC TryEnter()
//		{
//			CheckCancellationRequested();
//			TaskCompletionSource<object> access = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
//			Status status;

//			using (Lock.Enter())
//			{
//				if (_access == null)
//				{
//					_access = access;
//					status = Status.HasEntry;
//				}
//				else
//				{
//					status = Status.WaitingForEntry;
//				}
//			}

//			return status == Status.HasEntry
//			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
//			: EntryBlockUC.RefusedEntry
//			;
//		}

//		public EntryBlockUC TryEnter(int milliseconds)
//		{
//			milliseconds = milliseconds < 0 ? 0 : milliseconds;
//			CheckCancellationRequested();
//			TaskCompletionSource<object> access = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
//			Status status;

//			using (Lock.Enter())
//			{
//				if (_access == null)
//				{
//					_access = access;
//					status = Status.HasEntry;
//				}
//				else
//				{
//					Queue.Enqueue(access);
//					status = Status.WaitingForEntry;
//				}
//			}

//			if (status == Status.HasEntry) return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);

//			Task.WhenAny(CancellationTask, access.Task, Task.Delay(milliseconds)).Wait();
//			CheckCancellationRequested();

//			using (Lock.Enter())
//			{
//				status = access.Task.IsCompleted && access.Task.Status == TaskStatus.RanToCompletion
//				? Status.HasEntry
//				: Status.WaitingForEntry
//				;
//			}
//			return status == Status.HasEntry
//			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
//			: EntryBlockUC.RefusedEntry
//			;
//		}
//	}
//}
