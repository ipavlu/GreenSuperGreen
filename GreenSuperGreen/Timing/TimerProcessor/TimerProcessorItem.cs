using System;
using System.Threading.Tasks;
using GreenSuperGreen.Async;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

namespace GreenSuperGreen.Timing
{
	public enum TimerExpiryAction
	{
		TrySetCanceled,
		TrySetResult
	}

	/// <summary>
	/// Equality is based on <see cref="TimingTCS"/>
	/// </summary>
	public struct TimerProcessorItem : IEquatable<TimerProcessorItem>
	{
		public DateTime? Expiry { get; }
		public TimerExpiryAction? ExpiryAction { get; }
		private object Result { get; }

		public object TimingTCS { get; }
		private ITaskCompletionSourceAccessor AccessorTimingTCS { get; }

		private Task TaskTCS => AccessorTimingTCS?.GetTask(TimingTCS);

		public bool? Expired { get { Task t = TaskTCS; return t == null ? (bool?)null : t.IsCompleted || t.IsCanceled || t.IsFaulted; } }

		public bool? TryExpire()
		{
			if (ExpiryAction == null) return AccessorTimingTCS?.TrySetCanceled(TimingTCS);
			if (ExpiryAction == TimerExpiryAction.TrySetCanceled) return AccessorTimingTCS?.TrySetCanceled(TimingTCS);
			if (ExpiryAction == TimerExpiryAction.TrySetResult) return AccessorTimingTCS?.TrySetResult(TimingTCS, Result);
			throw new ArgumentException($"Unexpected value of type TimerExpiryAction?: {ExpiryAction}");
		}

		public bool? TryCancel() => AccessorTimingTCS?.TrySetCanceled(TimingTCS);

		public bool? TryDispose() => TrySetException(TimerProcessor.DisposedException);

		public bool? TrySetException(Exception ex) => AccessorTimingTCS?.TrySetException(TimingTCS, ex);

		public bool? TrySetException(string msg) => AccessorTimingTCS?.TrySetException(TimingTCS, new Exception(msg));

		public static TimerProcessorItem Add<TArg>(DateTime Now, TimeSpan Delay)
		=> new TimerProcessorItem(	Now,
									Delay,
									new TaskCompletionSource<TArg>(TaskCreationOptions.RunContinuationsAsynchronously),
									TaskCompletionSourceAccessor<TArg>.Default,
									TimerExpiryAction.TrySetCanceled,
									null)
		;

		public static TimerProcessorItem AddWithResult<TArg>(DateTime Now, TimeSpan Delay, TArg result)
		=> new TimerProcessorItem(	Now,
									Delay,
									new TaskCompletionSource<TArg>(TaskCreationOptions.RunContinuationsAsynchronously),
									TaskCompletionSourceAccessor<TArg>.Default,
									TimerExpiryAction.TrySetResult,
									result)
		;

		public static TimerProcessorItem Remove<TArg>(TaskCompletionSource<TArg> tcs) 
		=> new TimerProcessorItem(tcs, TaskCompletionSourceAccessor<TArg>.Default)
		;

		private TimerProcessorItem(DateTime Now, TimeSpan Delay, object TimingTCS, ITaskCompletionSourceAccessor AccessorTimingTCS, TimerExpiryAction? expAction, object expResult)
		{
			this.ExpiryAction = expAction;
			this.Result = expResult;
			this.Expiry = Now + Delay;
			this.TimingTCS = TimingTCS ?? throw new ArgumentNullException(nameof(TimingTCS));
			this.AccessorTimingTCS = AccessorTimingTCS ?? throw new ArgumentNullException(nameof(AccessorTimingTCS));
			if ((Now + TimerProcessor.MinDelay) >= Expiry) TryExpire();
		}

		private TimerProcessorItem(object TimingTCS, ITaskCompletionSourceAccessor AccessorTimingTCS)
		{
			this.ExpiryAction = null;
			this.Result = null;
			this.Expiry = null;
			this.TimingTCS = TimingTCS ?? throw new ArgumentNullException(nameof(TimingTCS));
			this.AccessorTimingTCS = AccessorTimingTCS ?? throw new ArgumentNullException(nameof(AccessorTimingTCS));
		}

		public bool Equals(TimerProcessorItem other) => Equals(TimingTCS, other.TimingTCS);

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TimerProcessorItem item && Equals(item);
		}

		public override int GetHashCode() => TimingTCS?.GetHashCode() ?? 0;

		public static bool operator ==(TimerProcessorItem x, TimerProcessorItem y) => x.Equals(y);
		public static bool operator !=(TimerProcessorItem x, TimerProcessorItem y) => !x.Equals(y);
	}
}