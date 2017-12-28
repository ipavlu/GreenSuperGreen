using System.Threading.Tasks;
using GreenSuperGreen.Queues;

using NUnit.Framework;

namespace AwaitableConcurrentPriorityQueues
{
	[TestFixture]
	public class MultipleConsumersOverridingPriority
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

			Task[] tasks =
			{
				Task.Run(() => Higher(pQueue)),
				Task.Run(() => Lower(pQueue)),
				Task.Run(() => Normal(pQueue))
			};

			//note: consumers will await enqueued data by producer(s) instead burning CPU cycles with TryDequeue on empty collection!!!

			pQueue.Enqueue(QueuePriority.Lower, "1Lower");
			pQueue.Enqueue(QueuePriority.Higher, "1Higher");
			pQueue.Enqueue(QueuePriority.Normal, "1Normal");

			pQueue.Enqueue(QueuePriority.Lower, "2Lower");
			pQueue.Enqueue(QueuePriority.Normal, "2Normal");
			pQueue.Enqueue(QueuePriority.Higher, "2Higher");

			pQueue.Enqueue(QueuePriority.Lower, "3Lower");
			pQueue.Enqueue(QueuePriority.Higher, "3Higher");
			pQueue.Enqueue(QueuePriority.Normal, "3Normal");

			await Task.WhenAll(tasks);
		}

		private async Task Higher(PriorityQueueNotifierUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Higher)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Higher", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Higher)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Higher", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Higher)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Higher", dequeued);
		}

		private async Task Normal(PriorityQueueNotifierUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Normal)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Normal", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Normal)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Normal", dequeued);

			await pQueue.EnqueuedItemsAsync();//awaiting first not required, TryDequeue can come first as well
			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Normal)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Normal", dequeued);
		}

		private async Task Lower(PriorityQueueNotifierUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Lower)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("1Lower", dequeued);

			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Lower)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("2Lower", dequeued);

			while (!pQueue.TryDequeu(out dequeued, QueuePriority.Lower)) await pQueue.EnqueuedItemsAsync();
			Assert.AreEqual("3Lower", dequeued);
		}
	}
}
