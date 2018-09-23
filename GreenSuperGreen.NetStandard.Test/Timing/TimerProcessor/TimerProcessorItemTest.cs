using System;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing.Test
{
	[TestFixture]
	public class TimerProcessorItemTest
	{
		[Test]
		public void Uninitialized()
		{
			var aa = new TimerProcessorItem();
			var bb = new TimerProcessorItem();
			Assert.IsTrue(aa == bb);//uninitialized items are same, they are missing TaskCompletionSource
		}

		[Test]
		public void Initialized()
		{
			var aa = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.Zero);
			var bb = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.Zero);
			Assert.IsTrue(aa != bb);
		}

		[Test]
		public void Equals()
		{
			object aa = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.Zero);
			// ReSharper disable EqualExpressionComparison
			Assert.IsTrue(aa.Equals(aa));
			// ReSharper restore EqualExpressionComparison
		}

		[Test]
		public void GetHashCodeTest()
		{
			Assert.AreEqual(0, new TimerProcessorItem().GetHashCode());
			Assert.AreEqual(new TimerProcessorItem().GetHashCode(), new TimerProcessorItem().GetHashCode());
			Assert.AreNotEqual(	TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.FromSeconds(1)),
								TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void TrySetException()
		{
			var item = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.FromSeconds(1));
			item.TrySetException("ex");
			Assert.IsNotNull(item.Expired);
			Assert.IsTrue(item.Expired.Value);
		}

		[Test]
		public void TrySetException2()
		{
			var item = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.FromSeconds(1));
			item.TrySetException(new Exception("ex"));
			Assert.IsNotNull(item.Expired);
			Assert.IsTrue(item.Expired.Value);
		}

		[Test]
		public void TryCancel()
		{
			var item = TimerProcessorItem.Add<object>(DateTime.Now, TimeSpan.FromSeconds(1));
			item.TryCancel();
			Assert.IsNotNull(item.Expired);
			Assert.IsTrue(item.Expired.Value);
		}
	}
}
