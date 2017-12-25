using NUnit.Framework;

// ReSharper disable RedundantCast
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.IdentifierGenerators.Test
{
	public partial class IUniqueIDTest
	{
		private class Uniqueness : AUniqueID<Uniqueness> { }

		[Test]
		public void CheckUniqueness()
		{
			Uniqueness uniqueA1 = new Uniqueness();
			Uniqueness uniqueA2 = new Uniqueness();

			Assert.AreNotEqual(uniqueA1.UniqueID, uniqueA2.UniqueID);
			Assert.AreNotEqual(uniqueA1.GetHashCode(), uniqueA2.GetHashCode());

			Assert.AreEqual(uniqueA1, (object)uniqueA1);
			Assert.AreEqual((IUniqueID)uniqueA1, (object)uniqueA1);
			Assert.AreEqual(uniqueA1.UniqueID, uniqueA1.GetHashCode());
		}
	}
}
