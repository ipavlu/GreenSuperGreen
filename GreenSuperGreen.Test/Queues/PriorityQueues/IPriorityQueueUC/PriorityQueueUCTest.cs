using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues.Test
{
	[TestFixture]
	public class PriorityQueueUCTest
	{
		enum Priorities
		{
			Top,
			Medium,
			Bottom
		}



		[Test]
		public void BasicFunctionality()
		{
			Priorities[] descendingPriorityOrder =
			{
				Priorities.Top,
				Priorities.Medium,
				Priorities.Bottom
			};

			PriorityQueueUC<Priorities, Priorities> priorityQueues =
			new PriorityQueueNotifierUC<Priorities, Priorities>(descendingPriorityOrder)
			;

			priorityQueues.Enqueue(Priorities.Bottom, Priorities.Bottom);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			priorityQueues.Enqueue(Priorities.Top, Priorities.Top);

			Priorities item;

			Assert.IsTrue(priorityQueues.TryDequeu(out item));
			Assert.AreEqual(item, Priorities.Top);

			Assert.IsTrue(priorityQueues.TryDequeu(out item));
			Assert.AreEqual(item, Priorities.Medium);

			Assert.IsTrue(priorityQueues.TryDequeu(out item));
			Assert.AreEqual(item, Priorities.Medium);

			Assert.IsTrue(priorityQueues.TryDequeu(out item));
			Assert.AreEqual(item, Priorities.Medium);

			Assert.IsTrue(priorityQueues.TryDequeu(out item));
			Assert.AreEqual(item, Priorities.Bottom);


			Assert.AreEqual(priorityQueues.Count(), 0);

			priorityQueues.Enqueue(Priorities.Medium, Priorities.Medium);
			Assert.AreEqual(priorityQueues.Count(Priorities.Top), 0);
			Assert.AreEqual(priorityQueues.Count(Priorities.Medium), 1);
			Assert.AreEqual(priorityQueues.Count(Priorities.Bottom), 0);

		}
	}
}
