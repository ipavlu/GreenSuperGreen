using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues.Test
{
	[TestFixture]
	public class PriorityQueueNotifierUCTest
	{
		private enum Priorities
		{
			Top,
			Medium,
			Bottom
		}

		private Priorities[] DescendingPriorityOrder { get; } =
		{
			Priorities.Top,
			Priorities.Medium,
			Priorities.Bottom
		};

		[Test]
		public async Task BasicFunctionality()
		{
			PriorityQueueNotifierUC<Priorities, Priorities> priorityQueues =
			new PriorityQueueNotifierUC<Priorities, Priorities>(DescendingPriorityOrder)
			;

			priorityQueues.Enqueue(Priorities.Bottom, Priorities.Bottom);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Top, Priorities.Top);
			priorityQueues.Enqueue(Priorities.Bottom, Priorities.Bottom);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Top, Priorities.Top);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Top, Priorities.Top);

			Queue<Priorities> dequeue = new Queue<Priorities>();

			await TestNotification(priorityQueues, dequeue);

			Assert.AreEqual(dequeue.Dequeue(), Priorities.Top);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Top);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Top);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Medium);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Medium);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Medium);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Bottom);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Bottom);

			Assert.AreEqual(priorityQueues.Count(), 0);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			Assert.AreEqual(priorityQueues.Count(), 1);

			Assert.AreEqual(priorityQueues.Count(Priorities.Top), 0);
			Assert.AreEqual(priorityQueues.Count(Priorities.Medium), 1);
			Assert.AreEqual(priorityQueues.Count(Priorities.Bottom), 0);

			await TestNotification(priorityQueues, dequeue);
			Assert.AreEqual(dequeue.Dequeue(), Priorities.Medium);
		}

		private
		async
		Task
		TestNotification(	PriorityQueueNotifierUC<Priorities, Priorities> priorityQueues,
							Queue<Priorities> dequeue)
		{
			while (priorityQueues.Count() > 0)
			{
				Priorities priorities;
				await priorityQueues.EnqueuedItemsAsync();
				if(!priorityQueues.TryDequeu(out priorities)) continue;
				dequeue.Enqueue(priorities);
			}
		}
	}
}