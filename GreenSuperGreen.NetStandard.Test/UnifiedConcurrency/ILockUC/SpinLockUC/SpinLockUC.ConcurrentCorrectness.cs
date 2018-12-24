using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.UnifiedConcurrency.Test
{
	public partial class SpinLockUCTest
	{
		[Test]
		public void ConcurrentCorrectness()
		{
			ILockUC spinlock = new SpinLockUC();

			int index = 0;

			Task[] tasks =
			Enumerable
			.Range(0, Math.Max(Environment.ProcessorCount, 4))
			.Select(x => Task.Run(() =>
			{
				for (int i = 0; i < 100; i++)
				{
					using (spinlock.Enter())
					{
						Assert.AreEqual(Interlocked.Increment(ref index), 1);
						Thread.Sleep(1);
						Assert.AreEqual(Interlocked.Decrement(ref index), 0);
					}
				}
			}))
			.ToArray()
			;

			Task.WaitAll(tasks);
		}
	}
}
