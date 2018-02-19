using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing.Test
{
	public class RealTimeSourceTest : IRealTimeSource
	{
		private ISimpleLockUC Lock { get; } = new SpinLockUC();

		private DateTime UtcNow { get; set; }
		private DateTime Now { get; set; }

		public DateTime GetUtcNow() { using (Lock.Enter()) { return UtcNow; } }
		public DateTime GetNow() { using (Lock.Enter()) { return Now; } }

		public void SetUtcNow(DateTime dt) { using (Lock.Enter()) { UtcNow = dt; } }
		public void SetNow(DateTime dt) { using (Lock.Enter()) { Now = dt; } }
	}

	[TestFixture]
	public class TimerProcessorTest
	{
		[Test]
		[Obsolete("Incomplete test")]
		public async Task Test()
		{
			var source = new RealTimeSourceTest();
			var tickGenerator = new TickGenerator(10);
			var timing = new TimerProcessor(source, tickGenerator);

		}
	}
}
