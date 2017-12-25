using System;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Console
{
	public static class ConsoleUC
	{
		public static async Task<ConsoleKeyInfo?> ReadEnter(int? timeout = null)
		{
		return await Task.Run(() => ReadEnterWorker(timeout));
		}

		private static async Task<ConsoleKeyInfo?> ReadEnterWorker(int? timeout = null)
		{
			Task timeoutTask = timeout.HasValue && timeout > 0 ? Task.Delay(timeout.Value) : null;

			while (true)
			{
				if (System.Console.KeyAvailable)
				{
					var character = System.Console.ReadKey(true);
					if (character.Key == ConsoleKey.Enter) return character;
				}

				if (timeoutTask?.IsCompleted == true) return null;
				await Task.Delay(100);
			}
		}

		public static Task<string> ReadLine() => Task.Run((Func<string>)System.Console.ReadLine);
	}
}
