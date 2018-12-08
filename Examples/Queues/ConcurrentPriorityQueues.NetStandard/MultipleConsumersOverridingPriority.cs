using System.Threading.Tasks;
using GreenSuperGreen.Queues;

using NUnit.Framework;

namespace ConcurrentPriorityQueues
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

			var pQueue = new PriorityQueueUC<QueuePriority, string>(descendingPriorities);

			pQueue.Enqueue(QueuePriority.Lower, "1Lower");
			pQueue.Enqueue(QueuePriority.Higher, "1Higher");
			pQueue.Enqueue(QueuePriority.Normal, "1Normal");

			pQueue.Enqueue(QueuePriority.Lower, "2Lower");
			pQueue.Enqueue(QueuePriority.Normal, "2Normal");
			pQueue.Enqueue(QueuePriority.Higher, "2Higher");

			pQueue.Enqueue(QueuePriority.Lower, "3Lower");
			pQueue.Enqueue(QueuePriority.Higher, "3Higher");
			pQueue.Enqueue(QueuePriority.Normal, "3Normal");

			Task[] tasks =
			{
				Task.Run(() => Higher(pQueue)),
				Task.Run(() => Lower(pQueue)),
				Task.Run(() => Normal(pQueue))
			};

			await Task.WhenAll(tasks);
		}

		private void Higher(PriorityQueueUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			pQueue.TryDequeu(out dequeued, QueuePriority.Higher);
			Assert.AreEqual("1Higher", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Higher);
			Assert.AreEqual("2Higher", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Higher);
			Assert.AreEqual("3Higher", dequeued);
		}

		private void Normal(PriorityQueueUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			pQueue.TryDequeu(out dequeued, QueuePriority.Normal);
			Assert.AreEqual("1Normal", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Normal);
			Assert.AreEqual("2Normal", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Normal);
			Assert.AreEqual("3Normal", dequeued);
		}

		private void Lower(PriorityQueueUC<QueuePriority, string> pQueue)
		{
			string dequeued;

			pQueue.TryDequeu(out dequeued, QueuePriority.Lower);
			Assert.AreEqual("1Lower", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Lower);
			Assert.AreEqual("2Lower", dequeued);

			pQueue.TryDequeu(out dequeued, QueuePriority.Lower);
			Assert.AreEqual("3Lower", dequeued);
		}
	}
}
