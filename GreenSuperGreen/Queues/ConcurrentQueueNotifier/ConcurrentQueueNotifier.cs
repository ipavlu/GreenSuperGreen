using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable RedundantStringInterpolation
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues
{
	public interface IConcurrentQueueNotifier<TItem>
	{
		int? ThrottleLevel { get; set; }

		Task EnqueueAsync(TItem item);
		Task EnqueueAsync(IList<TItem> items);
		Task EnqueueAsync(IEnumerable<TItem> items);
		Task EnqueuedItemsAsync();

		bool TryDequeu(out TItem item);

		int Count();
	}

	public class ConcurrentQueueNotifier<TItem> : IConcurrentQueueNotifier<TItem>
	{
		private ILockUC Lock { get; } = new SpinLockUC();
		private Queue<TItem> ItemsQueue { get; } = new Queue<TItem>();
		private Queue<TaskCompletionSource<object>> EnqueueAccess { get; } = new Queue<TaskCompletionSource<object>>();
		private Queue<TaskCompletionSource<object>> DequeueAccess { get; } = new Queue<TaskCompletionSource<object>>();

		private int? ThrottleLevelLimiter { get; set; }

		public int? ThrottleLevel
		{
			get => GetThrottleLevelLimiter();
			set => SetThrottleLevelLimiter(value);
		}

		public ConcurrentQueueNotifier(int? throttleLevel = null)
		{
			SetThrottleLevelLimiter(throttleLevel);
		}

		private int? GetThrottleLevelLimiter()
		{
			using (Lock.Enter()) return ThrottleLevelLimiter;
		}

		private int? SetThrottleLevelLimiter(int? throttleLevel = null)
		{
			using (Lock.Enter())
			{
				ThrottleLevelLimiter = throttleLevel <= 0 ? null : throttleLevel;
				LockedUpdateEnqueueAccess();
				return ThrottleLevelLimiter;
			}
		}

		private void LockedUpdateEnqueueAccess()
		{
			for (int i = ItemsQueue.Count; EnqueueAccess.Count > 0 && (ThrottleLevelLimiter == null || i < ThrottleLevelLimiter) ; i++)
			{
				TaskCompletionSource<object> tcs = EnqueueAccess.Dequeue();
				tcs?.SetResult(null);
			}
		}

		private Task LockedInsertUpdateAsync()
		{
			int maxConsumers = Math.Min(ItemsQueue.Count, DequeueAccess.Count);
			while (maxConsumers-- > 0)
			{
				TaskCompletionSource<object> tcs = DequeueAccess.Dequeue();
				tcs?.SetResult(null);
			}

			if (ThrottleLevelLimiter == null || ThrottleLevelLimiter.Value > ItemsQueue.Count) return Task.CompletedTask;
			var tcs2 = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
			EnqueueAccess.Enqueue(tcs2);
			return tcs2.Task;
		}

		public int Count()
		{
			using (Lock.Enter()) return ItemsQueue.Count;
		}

		public Task EnqueueAsync(TItem item)
		{
			using (Lock.Enter())
			{
				ItemsQueue.Enqueue(item);
				return LockedInsertUpdateAsync();
			}
		}

		public async Task EnqueueAsync(IList<TItem> items)
		{
			if (items == null || items.Count <= 0) return;
			for (int i = 0; i < items.Count; ++i)
			{
				await EnqueueAsync(items[i]);
			}
		}

		public async Task EnqueueAsync(IEnumerable<TItem> items)
		{
			foreach (TItem item in items ?? Enumerable.Empty<TItem>())
			{
				await EnqueueAsync(item);
			}
		}

		public Task EnqueuedItemsAsync()
		{
			using (Lock.Enter())
			{
				if (ItemsQueue.Count > 0) return Task.CompletedTask;
				var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
				DequeueAccess.Enqueue(tcs);
				return tcs.Task;
			}
		}

		public bool TryDequeu(out TItem item)
		{
			using (Lock.Enter())
			{
				if (ItemsQueue.Count <= 0)
				{
					item = default(TItem);
					return false;
				}

				item = ItemsQueue.Dequeue();
				LockedUpdateEnqueueAccess();
				return true;
			}
		}

		public override string ToString()
		{
			using (Lock.Enter())
			{
				return
				$"[ConcurrentQueueNotifier]" +
				$"[{nameof(ThrottleLevel)}:{ThrottleLevelLimiter?.ToString() ?? "none"}]" +
				$"[AwaitingEnqueues:{EnqueueAccess.Count}]" +
				$"[AwaitingDequeues:{DequeueAccess.Count}]" +
				$"[ItemsCount:{ItemsQueue.Count}]"
				;
			}
		}
	}
}
