using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen
{
	public static class TaskConstants
	{
		public static Task<bool> TaskTrue { get; } = Task.FromResult(true);
		public static Task<bool> TaskFalse { get; } = Task.FromResult(false);
		public static Task<bool?> TaskNullableTrue { get; } = Task.FromResult<bool?>(true);
		public static Task<bool?> TaskNullableFalse { get; } = Task.FromResult<bool?>(false);

		public static Task<int?> NullInt { get; } = Task.FromResult((int?)null);

		private static TaskCompletionSource<TArg> SetCanceledFunc<TArg>(this TaskCompletionSource<TArg> tcs) { tcs?.SetCanceled(); return tcs; }
		public static Task<object> TaskCanceledObject { get; } = new TaskCompletionSource<object>().SetCanceledFunc().Task;
		public static Task TaskCanceled => TaskCanceledObject;
	}
}
