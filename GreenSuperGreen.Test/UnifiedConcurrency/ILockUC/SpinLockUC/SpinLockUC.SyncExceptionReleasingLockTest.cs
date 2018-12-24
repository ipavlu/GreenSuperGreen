using System;
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
		public void SyncExceptionReleasingLockTest()
		{
			ILockUC spinlock = new SpinLockUC();
			Assert.Throws<Exception>(() => SyncExceptionTestMethod(spinlock));
			EntryBlockUC entry = spinlock.TryEnter();
			Assert.IsTrue(entry.HasEntry);
			entry.Dispose();
			entry = spinlock.TryEnter();
			Assert.IsTrue(entry.HasEntry);
			entry.Dispose();
		}

		public void SyncExceptionTestMethod(ILockUC spinlock)
		{
			using (spinlock.Enter())
			{
				throw new Exception();
			}
		}
	}
}
