using NUnit.Framework;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	[TestFixture]
	public class ConcurrencyLevelLimiterTest
	{
		[Test]
		public void BasicTestMAxLevel0()
		{
			var limiter = new ConcurrencyLevelLimiter(0);
			using (var entry1 = limiter.TryEnter())
			{
				Assert.IsFalse(entry1.HasEntry);
			}
		}


		[Test]
		public void BasicTestMAxLevel1()
		{
			var limiter = new ConcurrencyLevelLimiter(1);
			using (var entry1 = limiter.TryEnter())
			{
				Assert.IsTrue(entry1.HasEntry);

				using (var entry2 = limiter.TryEnter())
				{
					Assert.IsFalse(entry2.HasEntry);
				}
			}
		}

		[Test]
		public void BasicTestMAxLevel2()
		{
			var limiter = new ConcurrencyLevelLimiter(2);
			using (var entry1 = limiter.TryEnter())
			{
				Assert.IsTrue(entry1.HasEntry);

				using (var entry2 = limiter.TryEnter())
				{
					Assert.IsTrue(entry2.HasEntry);

					using (var entry3 = limiter.TryEnter())
					{
						Assert.IsFalse(entry3.HasEntry);
					}
				}
			}
		}
	}
}
