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
		public enum AwaitAheadOfDataEnum
		{
			Enqueue2ItemsBegin,
			Enqueue2ItemsA,
			Enqueue2ItemsEnd,

			EnqueuedItemsAsyncBegin,
			EnqueuedItemsAsyncAwaiting,
			EnqueuedItemsAsyncEnd,
		}

		public static async Task Enque2Items(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.Enqueue2ItemsBegin);

			await notifier.EnqueueAsync("A");
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.Enqueue2ItemsA);

			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.Enqueue2ItemsEnd);
		}

		public static async Task AwaitAhedOfData(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.EnqueuedItemsAsyncBegin);

			Task t = notifier.EnqueuedItemsAsync();
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.EnqueuedItemsAsyncAwaiting, t);
			await t;

			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAheadOfDataEnum.EnqueuedItemsAsyncEnd);
		}

		[Test]
		public async Task AwaitAheadOfDataTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(AwaitAheadOfDataEnum.Enqueue2ItemsBegin, new StrategyOneOnOneUC())
			.Register(AwaitAheadOfDataEnum.Enqueue2ItemsA, new StrategyOneOnOneUC())
			.Register(AwaitAheadOfDataEnum.Enqueue2ItemsEnd, new StrategyOneOnOneUC())
			.Register(AwaitAheadOfDataEnum.EnqueuedItemsAsyncBegin, new StrategyOneOnOneUC())
			.Register(AwaitAheadOfDataEnum.EnqueuedItemsAsyncAwaiting, new StrategyOneOnOneUC())
			.Register(AwaitAheadOfDataEnum.EnqueuedItemsAsyncEnd, new StrategyOneOnOneUC())
			;

			const int throttle = 3;

			IConcurrentQueueNotifier<string> notifier = new ConcurrentQueueNotifier<string>(throttle);

			sequencer.Run(() => Enque2Items(sequencer, notifier));
			sequencer.Run(() => AwaitAhedOfData(sequencer, notifier));

			await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.EnqueuedItemsAsyncBegin);

			IProductionPointUC point = await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.EnqueuedItemsAsyncAwaiting);
			Task t = point.ProductionArg as Task;
			Assert.IsFalse(t?.IsCompleted);

			await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.Enqueue2ItemsBegin);
			await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.Enqueue2ItemsA);
			await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.EnqueuedItemsAsyncEnd);
			await sequencer.TestPointCompleteAsync(AwaitAheadOfDataEnum.Enqueue2ItemsEnd);

			Assert.AreEqual(1, notifier.Count());

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}
	}
}