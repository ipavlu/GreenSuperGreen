using NUnit.Framework;

// ReSharper disable ConstantNullCoalescingCondition
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	public partial class SpinLockUCTest
	{
		[Test]
		public void SingleThreadCorrectness()
		{
			SpinLockUC spinlock = new SpinLockUC();

			using (spinlock.Enter())
			{
			}

			using (EntryBlockUC entry = spinlock.Enter())
			{
				Assert.AreEqual(entry.EntryTypeUC, EntryTypeUC.Exclusive);
				Assert.IsTrue(entry.HasEntry);
			}

			using (EntryBlockUC entry = spinlock.TryEnter())
			{
				Assert.AreEqual(entry.EntryTypeUC, EntryTypeUC.Exclusive);
				Assert.IsTrue(entry.HasEntry);
				if (entry.HasEntry)
				{
					
				}
			}

			using (EntryBlockUC entry = spinlock.TryEnter(1))
			{
				Assert.AreEqual(entry.EntryTypeUC, EntryTypeUC.Exclusive);
				Assert.IsTrue(entry.HasEntry);
				if (entry.HasEntry)
				{

				}
			}

			EntryBlockUC entryOutsideUsing1 = spinlock.Enter();
			EntryBlockUC entryOutsideUsing2 = spinlock.TryEnter();
			EntryBlockUC entryOutsideUsing3 = spinlock.TryEnter(1);

			Assert.IsTrue(entryOutsideUsing1.HasEntry);
			Assert.IsFalse(entryOutsideUsing2.HasEntry);
			Assert.IsFalse(entryOutsideUsing3.HasEntry);

			Assert.IsTrue(entryOutsideUsing2 == EntryBlockUC.RefusedEntry);

			entryOutsideUsing1.Dispose();

			using (IEntryBlockUC entry = spinlock.Enter())
			{
				Assert.IsTrue(entry.HasEntry);
			}

			using (IEntryBlockUC entry = spinlock.TryEnter())
			{
				Assert.IsTrue(entry.HasEntry);
			}

			using (IEntryBlockUC entry = spinlock.TryEnter(1))
			{
				Assert.IsTrue(entry.HasEntry);
			}
		}
	}
}
