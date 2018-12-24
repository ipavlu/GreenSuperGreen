using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using GreenSuperGreen.Async;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedMember.Local
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> Awaitable with <see cref="EntryBlockUC"/> result </summary>
	public struct AsyncEntryBlockUC : ISimpleCompletionUC, INotifyCompletion, ICriticalNotifyCompletion
	{
		public static AsyncEntryBlockUC RefusedEntry { get; } = new AsyncEntryBlockUC(EntryBlockUC.RefusedEntry, (TaskCompletionSource<EntryBlockUC>)null);

		private EntryBlockUC? EntryBlock { get; }
		private ConfiguredTaskAwaitable<EntryBlockUC>.ConfiguredTaskAwaiter? TaskAwaiter { get; }

		private EntryBlockUC? EntryBlockForTaskPredicate { get; }
		private ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter? TaskBoolPredicateAwaiter { get; }
		private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter? TaskPredicateAwaiter { get; }

		public AsyncEntryBlockUC(EntryBlockUC entryBlock) : this(entryBlock, (TaskCompletionSource<EntryBlockUC>)null) { }
		public AsyncEntryBlockUC(EntryTypeUC entryTypeUC, IEntryCompletionUC entryCompletion) : this(new EntryBlockUC(entryTypeUC, entryCompletion), (TaskCompletionSource<EntryBlockUC>)null) { }

		public AsyncEntryBlockUC(EntryBlockUC? entryBlock, TaskCompletionSource<EntryBlockUC> tcs, ConfigCompletionContinuation configContinuation = ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			EntryBlock = entryBlock;
			TaskAwaiter = tcs?.Task.ConfigureAwait(configContinuation.ContinueOnCapturedContext()).GetAwaiter();

			EntryBlockForTaskPredicate = null;
			TaskBoolPredicateAwaiter = null;
			TaskPredicateAwaiter = null;

			if (!EntryBlock.HasValue && !TaskAwaiter.HasValue) throw new ArgumentNullException($"{nameof(entryBlock)}, {nameof(tcs)}");
		}

		public AsyncEntryBlockUC(EntryBlockUC entryBlockForTaskPredicate, Task<bool> taskPredicate, ConfigCompletionContinuation configContinuation = ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			EntryBlock = null;
			TaskAwaiter = null;

			EntryBlockForTaskPredicate = entryBlockForTaskPredicate;
			TaskBoolPredicateAwaiter = taskPredicate.ConfigureAwait(configContinuation.ContinueOnCapturedContext()).GetAwaiter();
			TaskPredicateAwaiter = null;

			if (!TaskBoolPredicateAwaiter.HasValue) throw new ArgumentNullException(nameof(taskPredicate));
		}

		public AsyncEntryBlockUC(EntryBlockUC entryBlockForTaskPredicate, Task taskPredicate, ConfigCompletionContinuation configContinuation = ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			EntryBlock = null;
			TaskAwaiter = null;

			EntryBlockForTaskPredicate = entryBlockForTaskPredicate;
			TaskBoolPredicateAwaiter = null;
			TaskPredicateAwaiter = taskPredicate.ConfigureAwait(configContinuation.ContinueOnCapturedContext()).GetAwaiter();

			if (!TaskPredicateAwaiter.HasValue) throw new ArgumentNullException(nameof(taskPredicate));
		}


		public bool IsCompleted =>
		EntryBlock.HasValue ||
		(TaskAwaiter?.IsCompleted ?? false) ||
		(TaskBoolPredicateAwaiter?.IsCompleted ?? false) ||
		(TaskPredicateAwaiter?.IsCompleted ?? false)
		;

		public EntryBlockUC GetResult()
		{
			if (EntryBlock.HasValue) return EntryBlock.Value;
			if (TaskAwaiter.HasValue) return TaskAwaiter.Value.GetResult();
			if (TaskBoolPredicateAwaiter.HasValue) return TaskBoolPredicateAwaiter.Value.GetResult() ? EntryBlockForTaskPredicate ?? EntryBlockUC.RefusedEntry : EntryBlockUC.RefusedEntry;
			if (TaskPredicateAwaiter != null)
			{
				TaskPredicateAwaiter.Value.GetResult();
				return EntryBlockForTaskPredicate ?? EntryBlockUC.RefusedEntry;
			}

			return EntryBlockUC.RefusedEntry;
		}

		public AsyncEntryBlockUC GetAwaiter() => this;

		/// <summary>
		/// Compiler services first calling GetAwaiter().
		/// The returned awaiter must implement OnCompleted method, but it does not have to be called by CompilerServices!
		/// If the work based on IsCompleted is already completed, it can skip this step and execute continuation right away.
		///	DEPENDING ON COMPILER SERVICES ALWAYS CALLING THIS METHOD IS ILL ADVISED.
		/// </summary>
		public void OnCompleted(Action continuation)
		{
			TaskAwaiter?.OnCompleted(continuation);
			TaskBoolPredicateAwaiter?.OnCompleted(continuation);
			TaskPredicateAwaiter?.OnCompleted(continuation);
		}

		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation)
		{
			TaskAwaiter?.UnsafeOnCompleted(continuation);
			TaskBoolPredicateAwaiter?.UnsafeOnCompleted(continuation);
			TaskPredicateAwaiter?.UnsafeOnCompleted(continuation);
		}
	}
}