using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	public interface ITaskCompletionSourceAccessor
	{
		Task GetTask(object TCS);

		bool TrySetException(object TCS, Exception exception);
		bool TrySetException(object TCS, IEnumerable<Exception> exceptions);

		bool TrySetCanceled(object TCS);
		bool TrySetCanceled(object TCS, CancellationToken cancellationToken);

		bool TrySetResult(object TCS, object result);

		void SetException(object TCS, Exception exception);
		void SetException(object TCS, IEnumerable<Exception> exceptions);

		void SetCanceled(object TCS);

		void SetResult(object TCS, object result);
	}

	/// <summary>
	/// Access to generic <see cref="TaskCompletionSource{TResult}"/> in non generic way
	/// </summary>
	public class TaskCompletionSourceAccessor<TArg> : ITaskCompletionSourceAccessor
	{
		public static TaskCompletionSourceAccessor<TArg> Default { get; } = new TaskCompletionSourceAccessor<TArg>();

		public Task GetTask(object TCS) => (TCS as TaskCompletionSource<TArg>)?.Task;
		public bool TrySetException(object TCS, Exception exception) => (TCS as TaskCompletionSource<TArg>)?.TrySetException(exception) ?? false;
		public bool TrySetException(object TCS, IEnumerable<Exception> exceptions) => (TCS as TaskCompletionSource<TArg>)?.TrySetException(exceptions) ?? false;

		public bool TrySetCanceled(object TCS) => (TCS as TaskCompletionSource<TArg>)?.TrySetCanceled() ?? false;
		public bool TrySetCanceled(object TCS, CancellationToken cancellationToken) => (TCS as TaskCompletionSource<TArg>)?.TrySetCanceled(cancellationToken) ?? false;

		public bool TrySetResult(object TCS, object result) => (TCS as TaskCompletionSource<TArg>)?.TrySetResult((TArg)result) ?? false;

		public void SetException(object TCS, Exception exception) => (TCS as TaskCompletionSource<TArg>) ?.SetException(exception);
		public void SetException(object TCS, IEnumerable<Exception> exceptions) => (TCS as TaskCompletionSource<TArg>)?.SetException(exceptions);

		public void SetCanceled(object TCS) => (TCS as TaskCompletionSource<TArg>)?.SetCanceled();

		public void SetResult(object TCS, object result) => (TCS as TaskCompletionSource<TArg>)?.SetResult((TArg) result);
	}
}
