using System.Collections.Generic;
using System.Linq;

namespace GreenSuperGreen.Benchmarking
{
	public static class MedianExtension
	{
		public static double Median(this IEnumerable<double> ie)
		{
			double[] sortedNumbers = ie.OrderBy(n => n).ToArray();
			int halfIndex = sortedNumbers.Length / 2;
			double median = (sortedNumbers.Length % 2) == 0
					? (sortedNumbers[halfIndex] + sortedNumbers[halfIndex - 1]) / 2.0
					: sortedNumbers[halfIndex]
				;
			return median;
		}
	}
}