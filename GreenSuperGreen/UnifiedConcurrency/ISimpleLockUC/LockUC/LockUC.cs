using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// <para/> <see cref="SpinLockUC"/> is based on .Net <see cref="System.Threading.SpinLock"/>.
	/// <para/> Does not support reentrancy and does not protect against reentrancy!
	/// <para/> Enter and Exit can be done on different threads, but same thread should be preffered...
	/// </summary>
	public partial class LockUC : ISimpleLockUC
	{
		private Queue<LazyAccess> Queue { get; } = new Queue<LazyAccess>();
		private EntryCompletionUC EntryCompletion { get; }
		private Status LockStatus { get; set; } = Status.Opened;

		/// <summary> CAN NOT BE READONLY FIELD!!! CAN NOT BE PROPERTY!!! CAN NOT TRACK THREAD!!! </summary>0
		private SpinLock _spinLock = new SpinLock(false);

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonReentrant
		| SyncPrimitiveCapabilityUC.NonThreadAffine
		;

		public LockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit()
		{
			while (true)
			{
				LazyAccess access;
				bool gotLock = false;
				try
				{
					_spinLock.Enter(ref gotLock);
					if (Queue.Count == 0)
					{
						LockStatus = Status.Opened;
						return;
					}
					access = Queue.Dequeue();
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}
				if (access.Activate()) return;
			}
		}

		private enum Status { Opened, Locked }

		public EntryBlockUC Enter()
		{
			LazyAccess access;
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
				}
				Queue.Enqueue(access = new LazyAccess());
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}

			access.Wait();
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public EntryBlockUC TryEnter()
		{
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Locked) return EntryBlockUC.RefusedEntry;
				LockStatus = Status.Locked;
				return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			LazyAccess access;
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
				}
				Queue.Enqueue(access = new LazyAccess());
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}

			return access.Wait(milliseconds)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}
	}

	public partial class LockUC
	{
		private class LazyAccess
		{
			private static TaskCompletionSource<object> CompletedAccess { get; } = CreateCompletedAccess();
			private static TaskCompletionSource<object> CancelledAccess { get; } = CreateCancelledAccess();

			//special snowflake!
			private static TaskCompletionSource<object> CreateAccess() => new TaskCompletionSource<object>();

			private static TaskCompletionSource<object> CreateCompletedAccess()
			{
				TaskCompletionSource<object> completed = CreateAccess();
				completed.SetResult(null);
				return completed;
			}

			private static TaskCompletionSource<object> CreateCancelledAccess()
			{
				TaskCompletionSource<object> cancelled = CreateAccess();
				cancelled.SetCanceled();
				return cancelled;
			}
			/// <summary> CAN NOT BE READONLY FIELD!!! CAN NOT BE PROPERTY!!! CAN NOT TRACK THREAD!!! </summary>0
			private SpinLock _spinLock = new SpinLock(false);
			private TaskCompletionSource<object> Access { get; set; }

			public void Wait()
			{
				Task task;
				bool gotLock = false;

				try
				{
					_spinLock.Enter(ref gotLock);
					task = (Access = Access ?? CreateAccess()).Task;
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}

				task.Wait();
			}
			
			public bool Wait(int milliseconds)
			{
				if (milliseconds <= 0) return false;

				Task task;
				bool gotLock = false;

				try
				{
					_spinLock.Enter(ref gotLock);
					task = (Access = Access ?? CreateAccess()).Task;

					if (task.IsCompleted)
					{
						Access = CompletedAccess;
						return true;
					}

					if (task.IsCanceled) 
					{
						Access = CancelledAccess;
						return false;
					}
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}

				Task
				.WhenAny(task, Task.Delay(milliseconds))
				.Wait()
				;

				try
				{
					_spinLock.Enter(ref gotLock);
					if (task.IsCompleted)
					{
						Access = CompletedAccess;
						return true;
					}

					Access = CancelledAccess;
					return false;
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}
			}

			public bool Activate()
			{
				bool gotLock = false;
				try
				{
					_spinLock.Enter(ref gotLock);
					if (Access == null)	
					{
						Access = CompletedAccess;
						return true;
					}
					if (Access.Task.IsCompleted || Access.Task.IsCanceled) return false;
					Access.SetResult(null);
					return true;
				}
				finally
				{
					if (gotLock) _spinLock.Exit(true);
				}
			}
		}
	}
}
