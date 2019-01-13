using System.Reflection;

namespace GreenSuperGreen.Benchmarking.AssemblyInfo
{
	public static class AssemblyInfo
	{
		public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

		public static AssemblyCopyrightAttribute Copyright { get; } = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
		public static AssemblyProductAttribute Product { get; } = Assembly.GetCustomAttribute<AssemblyProductAttribute>();
		public static AssemblyFileVersionAttribute Version { get; } = Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
	}
}
