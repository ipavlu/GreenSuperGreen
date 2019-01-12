using System;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public class BenchmarkGlobalSettings
	{
		public static TimeSpan TestingTimeSpan { get; } = TimeSpan.FromSeconds(10);
		public static int TestingSpins { get; } = 50000;
	}
}