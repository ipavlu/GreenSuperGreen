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
	public struct AsyncTimerProcessorResult<TArg> : ISimpleCompletionUC, INotifyCompletion, ICriticalNotifyCompletion
	{
		public TaskCompletionSource<TArg> TCS { get; }
		public Task<TaskCompletionSource<TArg>> WrappedTCS { get; }

		public async Task<Task<TaskCompletionSource<TArg>>> WrapIntoTask()
		{
			await WrappedTCS;
			return WrappedTCS;
		}

		private ConfiguredTaskAwaitable<TaskCompletionSource<TArg>>.ConfiguredTaskAwaiter TaskAwaiter { get; }

		public
		AsyncTimerProcessorResult(	TaskCompletionSource<TArg> tcs,
									Task<TaskCompletionSource<TArg>> wrappedTCS,
									ConfigCompletionContinuation configContinuation = ConfigCompletionContinuation.ContinueOnDefaultContext)
		{
			TCS = tcs;
			WrappedTCS = wrappedTCS;
			if (tcs == null) throw new ArgumentNullException(nameof(tcs));
			if (wrappedTCS == null) throw new ArgumentNullException(nameof(wrappedTCS));

			TaskAwaiter = wrappedTCS.ConfigureAwait(configContinuation.ContinueOnCapturedContext()).GetAwaiter();
		}

		public bool IsCompleted => TaskAwaiter.IsCompleted;

		public TaskCompletionSource<TArg> GetResult() => TaskAwaiter.GetResult();

		public AsyncTimerProcessorResult<TArg> GetAwaiter() => this;

		/// <summary>
		/// Compiler services first calling GetAwaiter().
		/// The returned awaiter must implement OnCompleted method, but it does not have to be called by CompilerServices!
		/// If the work based on IsCompleted is already completed, it can skip this step and execute continuation right away.
		///	DEPENDING ON COMPILER SERVICES ALWAYS CALLING THIS METHOD IS ILL ADVISED.
		/// </summary>
		public void OnCompleted(Action continuation) => TaskAwaiter.OnCompleted(continuation);

		[SecurityCritical]
		public void UnsafeOnCompleted(Action continuation) => TaskAwaiter.UnsafeOnCompleted(continuation);
	}
}