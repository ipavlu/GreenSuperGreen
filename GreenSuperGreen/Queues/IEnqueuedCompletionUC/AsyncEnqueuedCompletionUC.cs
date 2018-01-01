using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using GreenSuperGreen.Async;

// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace GreenSuperGreen.Queues
{
	public struct AsyncEnqueuedCompletionUC
	:	IEnqueuedCompletionUC,
		ISimpleCompletionUC,
		INotifyCompletion,
		ICriticalNotifyCompletion
	{
		public static AsyncEnqueuedCompletionUC AlreadyAsyncEnqueued { get; } = new AsyncEnqueuedCompletionUC(true);

		private TaskCompletionSource<object> TaskCompletionSource { get; }
		private ConfiguredTaskAwaitable<object>.ConfiguredTaskAwaiter ConfiguredTaskAwaiter { get; }

		public AsyncEnqueuedCompletionUC(bool enqueued)// : base(ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			if (enqueued)
			{
				TaskCompletionSource = null;
				ConfiguredTaskAwaiter = Task.FromResult<object>(null).ConfigureAwait(false).GetAwaiter();
			}
			else
			{
				TaskCompletionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
				ConfiguredTaskAwaiter = TaskCompletionSource.Task.ConfigureAwait(false).GetAwaiter();
			}
		}

		public void Enqueued() => TaskCompletionSource?.SetResult(null);

		public void OnCompleted(Action continuation) => ConfiguredTaskAwaiter.OnCompleted(continuation);
		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation) => ConfiguredTaskAwaiter.UnsafeOnCompleted(continuation);
		public bool IsCompleted => ConfiguredTaskAwaiter.IsCompleted;
		public int UniqueID => -1;
		public void GetResult() => ConfiguredTaskAwaiter.GetResult();
		public AsyncEnqueuedCompletionUC GetAwaiter() => this;
	}
}
