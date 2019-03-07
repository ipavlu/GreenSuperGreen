using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public static class MedianExtension
	{
		public static double Median(this IEnumerable<double> ie) => ie?.NullableMedian() ?? 0d;

		public static double? NullableMedian(this IEnumerable<double> ie)
		{
			if (ie == null) return null;
			double[] arr = ie.ToArray();
			if (arr.Length <= 0) return null;

			double[] sortedNumbers = arr.OrderBy(n => n).ToArray();
			int halfIndex = sortedNumbers.Length / 2;
			double median = (sortedNumbers.Length % 2) == 0
					? (sortedNumbers[halfIndex] + sortedNumbers[halfIndex - 1]) / 2.0
					: sortedNumbers[halfIndex]
				;
			return median;
		}
	}
}