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
		public enum AwaitAlreadyEnqueuedDataEnum
		{
			Enqueue2ItemsBegin,
			Enqueue2ItemsEnd,

			EnqueuedItemsAsyncBegin,
			EnqueuedItemsAsyncEnd,
		}

		public static async Task Insert2Items(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsBegin);
			await notifier.EnqueueAsync("A");
			await notifier.EnqueueAsync("B");
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsEnd);
		}

		public static async Task AwaitEnqueuedData(ISequencerUC sequencer, IConcurrentQueueNotifier<string> notifier)
		{
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncBegin);
			await notifier.EnqueuedItemsAsync();
			await sequencer.PointAsync(SeqPointTypeUC.Match, AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncEnd);
		}

		[Test]
		public async Task AwaitAlreadyEnqueuedDataTest()
		{
			ISequencerUC sequencer =
			SequencerUC
			.Construct()
			.Register(AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsBegin, new StrategyOneOnOneUC())
			.Register(AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsEnd, new StrategyOneOnOneUC())
			.Register(AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncBegin, new StrategyOneOnOneUC())
			.Register(AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncEnd, new StrategyOneOnOneUC())
			;

			const int throttle = 3;

			IConcurrentQueueNotifier<string> notifier = new ConcurrentQueueNotifier<string>(throttle);

			sequencer.Run(() => Insert2Items(sequencer, notifier));
			sequencer.Run(() => AwaitEnqueuedData(sequencer, notifier));

			await sequencer.TestPointCompleteAsync(AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsBegin);//enqueue data
			await sequencer.TestPointCompleteAsync(AwaitAlreadyEnqueuedDataEnum.Enqueue2ItemsEnd);//enqueue completed

			Assert.AreEqual(2, notifier.Count());

			await sequencer.TestPointCompleteAsync(AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncBegin);//begin awaiting enqueued data
			await sequencer.TestPointCompleteAsync(AwaitAlreadyEnqueuedDataEnum.EnqueuedItemsAsyncEnd);//enqueued data should be detected

			await sequencer.WhenAll();
			sequencer.TryReThrowException();
		}
	}
}