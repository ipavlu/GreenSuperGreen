using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> </summary>
	public class AsyncLockUC : IAsyncLockUC
	{
		private Queue<TaskCompletionSource<EntryBlockUC>> Queue { get; } = new Queue<TaskCompletionSource<EntryBlockUC>>();
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

		public AsyncLockUC() { EntryCompletion = new EntryCompletionUC(Exit); }

		private void Exit()
		{
			while (true)
			{
				TaskCompletionSource<EntryBlockUC> access;
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
				if (access.TrySetResult(new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion))) return;
			}
		}

		private enum Status { Opened, Locked }

		public AsyncEntryBlockUC Enter()
		{
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
				}
				TaskCompletionSource<EntryBlockUC> access;
				Queue.Enqueue(access = new TaskCompletionSource<EntryBlockUC>(TaskCreationOptions.RunContinuationsAsynchronously));
				return new AsyncEntryBlockUC(null, access);
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
		}

		public AsyncEntryBlockUC TryEnter()
		{
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Locked) return AsyncEntryBlockUC.RefusedEntry;
				LockStatus = Status.Locked;
				return new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
		}

		public AsyncEntryBlockUC TryEnter(int milliseconds)
		{
			TaskCompletionSource<EntryBlockUC> access;
			bool gotLock = false;
			try
			{
				_spinLock.Enter(ref gotLock);
				if (LockStatus == Status.Opened)
				{
					LockStatus = Status.Locked;
					return new AsyncEntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
				}
				Queue.Enqueue(access = new TaskCompletionSource<EntryBlockUC>(TaskCreationOptions.RunContinuationsAsynchronously));
			}
			finally
			{
				if (gotLock) _spinLock.Exit(true);
			}
			Timeout(milliseconds, access);
			return new AsyncEntryBlockUC(null, access);
		}

		private static async void Timeout(int milliseconds, TaskCompletionSource<EntryBlockUC> access)
		{
			await Task.Delay(milliseconds);
			access.TrySetResult(EntryBlockUC.RefusedEntry);
		}
	}
}
