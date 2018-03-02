using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen
{
	public static class TaskConstants
	{
		public static Task<bool> TaskTrue { get; } = Task.FromResult(true);
		public static Task<bool> TaskFalse { get; } = Task.FromResult(false);
		public static Task<bool?> TaskNullableTrue { get; } = Task.FromResult<bool?>(true);
		public static Task<bool?> TaskNullableFalse { get; } = Task.FromResult<bool?>(false);

		private static Task GetTaskCanceled()
		{
			var tcs = new TaskCompletionSource<object>();
			tcs.SetCanceled();
			return tcs.Task;
		}

		public static Task TaskCanceled { get; } = GetTaskCanceled();

		public static Task<int?> NullInt { get; } = Task.FromResult((int?)null);
	}
}
