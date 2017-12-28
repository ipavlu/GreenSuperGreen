using System.Threading.Tasks;
using GreenSuperGreen.Queues;

using NUnit.Framework;

namespace AwaitableConcurrentPriorityQueues
{
	[TestFixture]
	public class OneConsumerOverridingPriority
	{
		/// <summary> Strongly typed priorities </summary>
		public enum QueuePriority
		{
			Lower,
			Normal,
			Higher
		}

		[Test]
		public async Task Test()
		{
			// Explicitly defined descending order of priorities
			QueuePriority[] descendingPriorities =
			{
				QueuePriority.Higher,
				QueuePriority.Normal,
				QueuePriority.Lower
			};

			var pQueue = new PriorityQueueNotifierUC<QueuePriority, string>(descendingPriorities);

			pQueue.Enqueue(QueuePriority.Lower, "1Lower");
			pQueue.Enqueue(QueuePriority.Higher, "1Higher");
			pQueue.Enqueue(QueuePriority.Normal, "1Normal");

			pQueue.Enqueue(QueuePriority.Lower, "2Lower");
			pQueue.Enqueue(QueuePriority.Normal, "2Normal");
			pQueue.Enqueue(QueuePriority.Higher, "2Higher");

			pQueue.Enqueue(QueuePriority.Lower, "3Lower");
			pQueue.Enqueue(QueuePriority.Higher, "3Higher");
			pQueue.Enqueue(QueuePriority.Normal, "3Normal");

			await Task.Run(() => Descending(pQueue));
		}

		private async Task Descending(PriorityQueueNotifierUC<QueuePriority,string> pQueue)
		{
			string dequeued;

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Higher", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Higher", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Higher", dequeued);


			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Normal", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Normal", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Normal", dequeued);


			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Lower", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Lower", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			//awaiting EnqueuedItemsAsync must match priority with TryDequeue, here default descending order, otherwise CPU cycles can be burned needlesly
			while (!pQueue.TryDequeu(out dequeued)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Lower", dequeued);

			await Task.CompletedTask;
		}
	}
}
