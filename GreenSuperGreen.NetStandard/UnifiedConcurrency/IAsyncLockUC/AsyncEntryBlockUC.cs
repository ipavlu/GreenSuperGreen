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
		public static AsyncEntryBlockUC RefusedEntry { get; } = new AsyncEntryBlockUC(EntryBlockUC.RefusedEntry, null);

		private EntryBlockUC? EntryBlock { get; }
		private ConfiguredTaskAwaitable<EntryBlockUC>.ConfiguredTaskAwaiter? TaskAwaiter { get; }

		public AsyncEntryBlockUC(EntryBlockUC entryBlock) : this(entryBlock, null) { }
		public AsyncEntryBlockUC(EntryTypeUC entryTypeUC, IEntryCompletionUC entryCompletion) : this(new EntryBlockUC(entryTypeUC, entryCompletion), null) { }

		public AsyncEntryBlockUC(EntryBlockUC? EntryBlock, TaskCompletionSource<EntryBlockUC> tcs, ConfigCompletionContinuation configContinuation = ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			this.EntryBlock = EntryBlock;
			this.TaskAwaiter = tcs?.Task.ConfigureAwait(configContinuation.ContinueOnCapturedContext()).GetAwaiter();
			if (!this.EntryBlock.HasValue && !this.TaskAwaiter.HasValue) throw new ArgumentNullException($"{nameof(EntryBlock)}, {nameof(TaskAwaiter)}");
		}

		public bool IsCompleted => EntryBlock.HasValue || (TaskAwaiter?.IsCompleted ?? false);

		public EntryBlockUC GetResult() => EntryBlock ?? (TaskAwaiter?.GetResult() ?? EntryBlockUC.RefusedEntry);

		public AsyncEntryBlockUC GetAwaiter() => this;

		/// <summary>
		/// Compiler services first calling GetAwaiter().
		/// The returned awaiter must implement OnCompleted method, but it does not have to be called by CompilerServices!
		/// If the work based on IsCompleted is already completed, it can skip this step and execute continuation right away.
		///	DEPENDING ON COMPILER SERVICES ALWAYS CALLING THIS METHOD IS ILL ADVISED.
		/// </summary>
		public void OnCompleted(Action continuation)
		{
			if (EntryBlock.HasValue) throw new InvalidOperationException($"Unexpected call, is completed, this method should not be called!");
			TaskAwaiter?.OnCompleted(continuation);
		}

		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation)
		{
			if (EntryBlock.HasValue) throw new InvalidOperationException($"Unexpected call, is completed, this method should not be called!");
			TaskAwaiter?.UnsafeOnCompleted(continuation);
		}
	}
}