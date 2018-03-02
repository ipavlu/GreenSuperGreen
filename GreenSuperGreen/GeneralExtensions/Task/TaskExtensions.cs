using System;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Primary use in unit tests, code is simpler,
		/// but costs memory allocations and adds extra steps into await chain state machine
		/// </summary>
		public static async Task<Task> WrapIntoTask(this Task t)
		{
			if (t == null) throw new ArgumentNullException(nameof(t));
			await t;
			return t;
		}

		/// <summary>
		/// Primary use in unit tests, code is simpler,
		/// but costs memory allocations and adds extra steps into await chain state machine
		/// </summary>
		public static async Task<Task<TResult>> WrapIntoTask<TResult>(this Task<TResult> t)
		{
			if (t == null) throw new ArgumentNullException(nameof(t));
			await t;
			return t;
		}
	}
}
