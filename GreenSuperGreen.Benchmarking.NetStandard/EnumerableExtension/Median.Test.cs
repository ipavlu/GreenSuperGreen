//using System.Linq;
//using System.Collections.Generic;
//using NUnit.Framework;

//// ReSharper disable InconsistentNaming
//// ReSharper disable CheckNamespace

//namespace GreenSuperGreen.UnifiedConcurrency.Test
//{
//	public static class MedianExtension
//	{
//		public static double Median(this IEnumerable<double> ie) => ie?.NullableMedian() ?? 0d;

//		public static double? NullableMedian(this IEnumerable<double> ie)
//		{
//			if (ie == null) return null;
//			double[] arr = ie.ToArray();
//			if (arr.Length <= 0) return null;

//			double[] sortedNumbers = arr.OrderBy(n => n).ToArray();
//			int halfIndex = sortedNumbers.Length / 2;
//			double median = (sortedNumbers.Length % 2) == 0
//					? (sortedNumbers[halfIndex] + sortedNumbers[halfIndex - 1]) / 2.0
//					: sortedNumbers[halfIndex]
//				;
//			return median;
//		}
//	}

//	[TestFixture]
//	public class MedianExtensionTests
//	{
//		[Test]
//		public void MedianNull()
//		{
//			Assert.Zero(((IEnumerable<double>)null).Median());
//		}

//		[Test]
//		public void MedianEmpty()
//		{
//			Assert.Zero(Enumerable.Empty<double>().Median());
//		}

//		[Test]
//		public void Median_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d }.Median());
//		}

//		[Test]
//		public void Median_2_4()
//		{
//			Assert.AreEqual(3.0d, new[] { 2.0d, 4.0d }.Median());
//		}


//		[Test]
//		public void Median_2_4_6()
//		{
//			Assert.AreEqual(4.0d, new[] { 2.0d, 4.0d, 6.0d }.Median());
//		}

//		[Test]
//		public void Median_1_1_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d, 1.0d, 1.0d }.Median());
//		}

//		[Test]
//		public void Median_1_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d, 1.0d }.Median());
//		}



//		[Test]
//		public void NullableMedianNull()
//		{
//			Assert.IsNull(((IEnumerable<double>)null).NullableMedian());
//		}

//		[Test]
//		public void NullableMedianEmpty()
//		{
//			Assert.IsNull(Enumerable.Empty<double>().NullableMedian());
//		}

//		[Test]
//		public void NullableMedian_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d }.NullableMedian());
//		}

//		[Test]
//		public void NullableMedian_2_4()
//		{
//			Assert.AreEqual(3.0d, new[] { 2.0d, 4.0d }.NullableMedian());
//		}


//		[Test]
//		public void NullableMedian_2_4_6()
//		{
//			Assert.AreEqual(4.0d, new[] { 2.0d, 4.0d, 6.0d }.NullableMedian());
//		}

//		[Test]
//		public void NullableMedian_1_1_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d, 1.0d, 1.0d }.NullableMedian());
//		}

//		[Test]
//		public void NullableMedian_1_1()
//		{
//			Assert.AreEqual(1.0d, new[] { 1.0d, 1.0d }.NullableMedian());
//		}
//	}
//}