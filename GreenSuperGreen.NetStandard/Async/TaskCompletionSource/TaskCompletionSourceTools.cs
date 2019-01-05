using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	public static class TaskCompletionSourceTools<TArg>
	{
		public static TaskCompletionSource<TArg> Cancelled { get; } = CreateCancelled();
		public static TaskCompletionSource<TArg> CompletedDefault { get; } = CreateCompleted();

		private static TaskCompletionSource<TArg> CreateCancelled()
		{
			TaskCompletionSource<TArg> cancelled = new TaskCompletionSource<TArg>();
			cancelled.SetCanceled();
			return cancelled;
		}

		public static TaskCompletionSource<TArg> CreateCompleted(TArg arg = default(TArg))
		{
			TaskCompletionSource<TArg> completed = new TaskCompletionSource<TArg>();
			completed.SetResult(arg);
			return completed;
		}
	}
}
