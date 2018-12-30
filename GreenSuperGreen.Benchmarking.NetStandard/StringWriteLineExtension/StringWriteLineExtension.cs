namespace GreenSuperGreen.Benchmarking
{
	public static class StringWriteLineExtension
	{
		/// <summary>  Write string to <see cref="System.Console.WriteLine"/>  </summary>
		public static string WriteLine(this string msg)
		{
			System.Console.WriteLine(msg);
			return msg;
		}
	}
}