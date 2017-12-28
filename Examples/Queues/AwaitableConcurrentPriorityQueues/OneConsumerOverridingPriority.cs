using System.Threading.Tasks;
using GreenSuperGreen.Queues;

using NUnit.Framework;

namespace AwaitableConcurrentPriorityQueues
{
	[TestFixture]
	public class OneConsumer
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

			await Task.Run(() => Descending(pQueue));
		}

		private async Task Descending(PriorityQueueUC<QueuePriority,string> pQueue)
		{
			string dequeued;

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("1Higher", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("2Higher", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("3Higher", dequeued);


			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("1Normal", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("2Normal", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("3Normal", dequeued);


			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("1Lower", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("2Lower", dequeued);

			pQueue.TryDequeu(out dequeued);
			Assert.AreEqual("3Lower", dequeued);

			await Task.CompletedTask;
		}
	}
}
