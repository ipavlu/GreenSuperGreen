
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace GreenSuperGreen.Benchmarking
{
	public static class StringWriteLineExtension
	{
		/// <summary>  Write string to <see><cref>System.Console.WriteLine</cref></see> </summary>
		public static string WriteLine(this string msg)
		{
			System.Console.WriteLine(msg);
			return msg;
		}
	}
}