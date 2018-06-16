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
	:	ISimpleCompletionUC,
		INotifyCompletion,
		ICriticalNotifyCompletion
	{
		public static AsyncEnqueuedCompletionUC Completed { get; } = new AsyncEnqueuedCompletionUC(Task.CompletedTask);

		public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter ConfiguredAwaiter { get; }

		public AsyncEnqueuedCompletionUC(Task task)
		{
			ConfiguredAwaiter = task.ConfigureAwait(false).GetAwaiter();
		}

		[SecuritySafeCritical]
		public void OnCompleted(Action continuation) => ConfiguredAwaiter.OnCompleted(continuation);

		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation) => ConfiguredAwaiter.UnsafeOnCompleted(continuation);

		public bool IsCompleted => ConfiguredAwaiter.IsCompleted;

		public void GetResult() => ConfiguredAwaiter.GetResult();

		public AsyncEnqueuedCompletionUC GetAwaiter() => this;
	}
}
