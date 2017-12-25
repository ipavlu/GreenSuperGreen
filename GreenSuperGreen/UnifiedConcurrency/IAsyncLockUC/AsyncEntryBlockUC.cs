using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GreenSuperGreen.Async;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedMember.Local
// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> Awaitable with <see cref="EntryBlockUC"/> result </summary>
	public struct AsyncEntryBlockUC : ISimpleCompletionUC, INotifyCompletion
	{
		public static AsyncEntryBlockUC RefusedEntry { get; } = new AsyncEntryBlockUC(EntryBlockUC.RefusedEntry, null);

		private EntryBlockUC? EntryBlock { get; }
		private TaskAwaiter<EntryBlockUC>? TaskAwaiter { get; }

		public AsyncEntryBlockUC(EntryTypeUC entryTypeUC, IEntryCompletionUC entryCompletion) : this(new EntryBlockUC(entryTypeUC, entryCompletion), null) { }

		public AsyncEntryBlockUC(TaskAwaiter<EntryBlockUC>? TaskAwaiter) : this(null, TaskAwaiter) { }
		public AsyncEntryBlockUC(Task<EntryBlockUC> Task) : this(Task?.GetAwaiter()) { }
		public AsyncEntryBlockUC(TaskCompletionSource<EntryBlockUC> tcs) : this(tcs?.Task) { }

		public AsyncEntryBlockUC(EntryBlockUC? EntryBlock, TaskAwaiter<EntryBlockUC>? TaskAwaiter)
		{
			this.EntryBlock = EntryBlock;
			this.TaskAwaiter = TaskAwaiter;
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
			if (EntryBlock.HasValue) continuation?.Invoke();
			else
			{
				TaskAwaiter?.OnCompleted(continuation);
			}
		}
	}
}