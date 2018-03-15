using System;
using System.Threading.Tasks;
using GreenSuperGreen.Async;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

namespace GreenSuperGreen.Timing
{
	/// <summary>
	/// Equality is based on <see cref="TimingTCS"/>
	/// </summary>
	public struct TimerProcessorItem : IEquatable<TimerProcessorItem>
	{
		public DateTime? Expiry { get; }

		public object TimingTCS { get; }
		private ITaskCompletionSourceAccessor AccessorTimingTCS { get; }

		private Task TaskTCS => AccessorTimingTCS?.GetTask(TimingTCS);
		public bool? Expired { get { Task t = TaskTCS; return t == null ? (bool?)null : t.IsCompleted || t.IsCanceled || t.IsFaulted; } }
		public bool? TryExpire() => AccessorTimingTCS?.TrySetCanceled(TimingTCS);
		public bool? TryDispose() => AccessorTimingTCS?.TrySetException(TimingTCS, TimerProcessor.DisposedException);

		public static TimerProcessorItem Add<TArg>(DateTime Now, TimeSpan Delay)
		=> new TimerProcessorItem(	Now,
									Delay,
									new TaskCompletionSource<TArg>(TaskCreationOptions.RunContinuationsAsynchronously),
									TaskCompletionSourceAccessor<TArg>.Default)
		;

		public static TimerProcessorItem Remove<TArg>(TaskCompletionSource<TArg> tcs) 
		=> new TimerProcessorItem(tcs, TaskCompletionSourceAccessor<TArg>.Default)
		;

		private TimerProcessorItem(DateTime Now, TimeSpan Delay, object TimingTCS, ITaskCompletionSourceAccessor AccessorTimingTCS)
		{
			this.Expiry = Now + Delay;
			this.TimingTCS = TimingTCS;
			this.AccessorTimingTCS = AccessorTimingTCS;
			if ((Now + TimerProcessor.MinDelay) >= Expiry) TryExpire();
		}

		private TimerProcessorItem(object TimingTCS, ITaskCompletionSourceAccessor AccessorTimingTCS)
		{
			this.Expiry = null;
			this.TimingTCS = TimingTCS;
			this.AccessorTimingTCS = AccessorTimingTCS;
		}

		public bool Equals(TimerProcessorItem other) => Equals(TimingTCS, other.TimingTCS);

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TimerProcessorItem && Equals((TimerProcessorItem) obj);
		}

		public override int GetHashCode() => TimingTCS.GetHashCode();

		public static bool operator ==(TimerProcessorItem x, TimerProcessorItem y) => x.Equals(y);
		public static bool operator !=(TimerProcessorItem x, TimerProcessorItem y) => !x.Equals(y);
	}
}