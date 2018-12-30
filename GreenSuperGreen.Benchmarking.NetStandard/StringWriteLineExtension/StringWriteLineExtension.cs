namespace GreenSuperGreen.Benchmarking
{
	public static class StringWriteLineExtension
	{
		/// <summary>  Write string to <see cref="System.Console.WriteLine"/>  </summary>
		public static void WriteLine(this string msg) => System.Console.WriteLine(msg);
	}
}