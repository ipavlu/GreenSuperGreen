using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using GreenSuperGreen.Sequencing;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues.Test
{
	[TestFixture]
	public partial class ConcurrentQueueNotifierTest
	{
		public enum Insert3ItemsThrottleTestEnum
		{
			Enqueue3Items_Begin,
			Enqueue3Items_A,
			Enqueue3Items_B,
			Enqueue3Items_C_Throttle,
			Enqueue3Items_End,

			EnqueuedItemsAsyncBegin,
			EnqueuedItemsAsyncDataItem,
			EnqueuedItemsAsyncEnd,
		}

		public static async Task Insert3ItemsThrottle(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.Enqueue3Items_Begin);

			await notifier.EnqueueAsync("A");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.Enqueue3Items_A);

			await notifier.EnqueueAsync("B");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.Enqueue3Items_B);

			Task result = notifier.EnqueueAsync("C");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.Enqueue3Items_C_Throttle, result);
			await result;

			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.Enqueue3Items_End);
		}

		public static async Task AwaitDequeue3(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncBegin);

			string item;

			await notifier.EnqueuedItemsAsync();
			if (!notifier.TryDequeu(out item)) throw new Exception("Expected data");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem, item);

			await notifier.EnqueuedItemsAsync();
			if (!notifier.TryDequeu(out item)) throw new Exception("Expected data");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem, item);

			await notifier.EnqueuedItemsAsync();
			if (!notifier.TryDequeu(out item)) throw new Exception("Expected data");
			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem, item);

			await sequencer.PointAsync(SeqPointTypeUC.Match, Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncEnd);
		}

		[Test]
		public async Task Insert3ItemsThrottleTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(Insert3ItemsThrottleTestEnum.Enqueue3Items_Begin, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.Enqueue3Items_A, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.Enqueue3Items_B, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.Enqueue3Items_C_Throttle, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.Enqueue3Items_End, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncBegin, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem, new StrategyOneOnOneUC())
			.Register(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncEnd, new StrategyOneOnOneUC())
			;

			const int throttle = 3;

			IConcurrentQueueNotifier<string> notifier = new ConcurrentQueueNotifier<string>(throttle);

			sequencer.Run(() => Insert3ItemsThrottle(sequencer, notifier));
			sequencer.Run(() => AwaitDequeue3(sequencer, notifier));

			IProductionPointUC prodPoint;


			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.Enqueue3Items_Begin);
			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.Enqueue3Items_A);
			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.Enqueue3Items_B);
			prodPoint = await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.Enqueue3Items_C_Throttle);
			Assert.IsFalse((prodPoint?.ProductionArg as Task)?.IsCompleted);
			Assert.AreEqual(3, notifier.Count());

			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncBegin);

			prodPoint = await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem);
			Assert.AreEqual("A", prodPoint?.ProductionArg as string);

			//throttling should be deactivated, resuming
			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.Enqueue3Items_End);

			prodPoint = await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem);
			Assert.AreEqual("B", prodPoint?.ProductionArg as string);

			prodPoint = await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncDataItem);
			Assert.AreEqual("C", prodPoint?.ProductionArg as string);

			await sequencer.TestPointCompleteAsync(Insert3ItemsThrottleTestEnum.EnqueuedItemsAsyncEnd);

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}
	}
}