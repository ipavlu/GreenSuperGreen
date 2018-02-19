using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing
{
	public struct ConcurrencyLevelEntry : IDisposable
	{
		public static ConcurrencyLevelEntry NoEntry { get; } = new ConcurrencyLevelEntry();
		private Action ActionOnExit { get; }
		public bool HasEntry { get; }

		public ConcurrencyLevelEntry(Action actionOnExit)
		{
			ActionOnExit = actionOnExit;
			HasEntry = actionOnExit != null;
		}

		public void Dispose() => ActionOnExit?.Invoke();
	}

	

	public interface IConcurrencyLevelCounter
	{
		int MaxConcurrencyLevel { get; }
		int ConcurrencyLevel { get; }
		ConcurrencyLevelEntry TryEnter();
	}

	public class ConcurrencyLevelCounter : IConcurrencyLevelCounter
	{
		private ISimpleLockUC Lock { get; } = new SpinLockUC();
		private int ConcurrencyLevelValue { get; set; }
		public int ConcurrencyLevel { get { using (Lock.Enter()) return ConcurrencyLevelValue; } }
		public int MaxConcurrencyLevel { get; }
		private Action ActionOnExit { get; }

		public ConcurrencyLevelCounter(int maxConcurrency)
		{
			MaxConcurrencyLevel = maxConcurrency;
			ActionOnExit = Exit;
		}

		private void Exit()
		{
			using (Lock.Enter()) { --ConcurrencyLevelValue; }
		}

		public ConcurrencyLevelEntry TryEnter()
		{
			using (Lock.Enter())
			{
				if (ConcurrencyLevelValue > MaxConcurrencyLevel) return ConcurrencyLevelEntry.NoEntry;
				++ConcurrencyLevelValue;
				return new ConcurrencyLevelEntry(ActionOnExit);
			}
		}
	}



	public interface ITickGenerator : IDisposable
	{
		Task<int?> PeriodAsync { get; }
		Task<bool> DisposedAsync { get; }
		Task<bool> RegisterCallbackAsync(Action asyncCallback);
		Task<bool> UnRegisterCallbackAsync(Action asyncCallback);
	}

	public class TickGenerator : ITickGenerator, IDisposable
	{
		public static Task<bool> TaskFalse { get; } = Task.FromResult(false);
		public static Task<bool> TaskTrue { get; } = Task.FromResult(true);
		public static Task<int?> PeriodNullInt { get; } = Task.FromResult((int?) null);
		
		private enum Request { Register, UnRegister }

		private struct RequestItem
		{
			private Action AsyncAction { get; }
			private Request Request { get; }
			public TaskCompletionSource<bool> TCS { get; }

			public RequestItem(Request request, Action asyncAction)
			{
				Request = request;
				AsyncAction = asyncAction;
				TCS = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			}

			public void UpdateHashSet(HashSet<Action> hashSet, bool disposed = false)
			{
				if (hashSet == null) throw new ArgumentNullException(nameof(hashSet));

				if (Request == Request.Register && !disposed) { hashSet.Add(AsyncAction); TCS.SetResult(true); }
				else if (Request == Request.Register && disposed) TCS.SetResult(false);
				else if (Request == Request.UnRegister) { hashSet.Remove(AsyncAction); TCS.SetResult(true); }
				else
				{
					Exception ex = new Exception($"Unknown request: {Request}");
					TCS.SetException(ex);
					throw ex;
				}
			}
		}

		private ISimpleLockUC Lock { get; } = new SpinLockUC();
		public const int MaxCallbackConcurrency = 1;
		private IConcurrencyLevelCounter ConcurrencyLevelCounter { get; } = new ConcurrencyLevelCounter(MaxCallbackConcurrency);

		private Timer Timer { get; set; }
		private HashSet<Action> HashedActions { get; } = new HashSet<Action>();
		private Queue<RequestItem> Requests { get; } = new Queue<RequestItem>();
		private Action[] Actions { get; set; }

		public Task<int?> PeriodAsync { get; private set; }
		public Task<bool> DisposedAsync { get; private set; }

		public TickGenerator(int period)
		{
			Timer = new Timer(Callback, this, period = period < 1 ? 1 : period, period);
			PeriodAsync = Task.FromResult((int?)period);
			DisposedAsync = TaskFalse;
		}

		public Task<bool> RegisterCallbackAsync(Action asyncCallback) => Registration(Request.Register, asyncCallback);
		public Task<bool> UnRegisterCallbackAsync(Action asyncCallback) => Registration(Request.UnRegister, asyncCallback);

		private Task<bool> Registration(Request request, Action asyncAction)
		{
			if (asyncAction == null || DisposedAsync.Result) return TaskFalse;

			using (Lock.Enter())
			{
				if (DisposedAsync.Result) return TaskFalse;
				RequestItem item = new RequestItem(request, asyncAction);
				Requests.Enqueue(item);
				return item.TCS.Task;
			}
		}

		private static void Callback(object obj) => (obj as TickGenerator)?.Callback();

		private void Callback() 
		{
			if (DisposedAsync.Result) return;

			using (var entry = ConcurrencyLevelCounter.TryEnter())
			{
				if (!entry.HasEntry) return;
				//exclusive access
				using (Lock.Enter())
				{
					if (DisposedAsync.Result) return;

					while (Requests.Count > 0)
					{
						Requests.Dequeue().UpdateHashSet(HashedActions);
						Actions = null;
					}

					Actions = Actions ?? HashedActions.ToArray();

					for (int i = 0; i < Actions.Length; i++)
					{
						Actions[i].Invoke();
					}
				}
			}
		}

		public void Dispose()
		{
			if (DisposedAsync.Result) return;
			Timer timer;
			using (Lock.Enter())
			{
				if (DisposedAsync.Result) return;
				DisposedAsync = TaskTrue;
				//DisposedAsync has been set to true, rest after sing block is thread safe now
				PeriodAsync = PeriodNullInt;
				timer = Timer;
				Timer = null;
			}

			while (Requests.Count > 0)
			{
				//Unregistration ends correctly,
				//new Registertrations actions are set to false.
				Requests.Dequeue().UpdateHashSet(HashedActions, disposed: true);
			}

			timer?.Change(Timeout.Infinite, Timeout.Infinite);
			timer?.Dispose();
			HashedActions.Clear();
			Requests.Clear();
			Actions = Array.Empty<Action>();
		}
	}



	public interface IRealTimeSource
	{
		DateTime GetUtcNow();
	}

	public class RealTimeSource : IRealTimeSource
	{
		public DateTime GetUtcNow() => DateTime.UtcNow;
	}



	public class TimerProcessorItemComparer : IComparer<ITimingItem>
	{
		public static IComparer<ITimingItem> Default { get; } = new TimerProcessorItemComparer();

		private TimerProcessorItemComparer() { }

		public int Compare(ITimingItem x, ITimingItem y)
		{
			if (x == null) throw new ArgumentNullException(nameof(x));
			if (y == null) throw new ArgumentNullException(nameof(y));
			return Comparer<DateTime>.Default.Compare(x.CancelAtUTCTime, y.CancelAtUTCTime);
		}
	}



	public interface ITimingItem
	{
		object ObjectTCS { get; }
		DateTime CancelAtUTCTime { get; }
		bool IsCompleted { get; }
		bool TrySetCancel();
		bool TrySetException(Exception exception);
	}

	public interface ITimingItem<TArg> : ITimingItem
	{
		TaskCompletionSource<TArg> TCS { get; }
	}

	public class TimingItem<TArg> : ITimingItem<TArg>, ITimingItem
	{
		public TaskCompletionSource<TArg> TCS { get; }
		public object ObjectTCS => TCS;
		public DateTime CancelAtUTCTime { get; }
		public bool IsCompleted => TCS.Task.IsCompleted;
		public bool TrySetCancel() => TCS.TrySetCanceled();
		public bool TrySetException(Exception exception) => TCS.TrySetException(exception);

		public TimingItem(DateTime cancelAtUTCTime, TaskCompletionSource<TArg> tcs = null)
		{
			TCS = tcs ?? new TaskCompletionSource<TArg>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelAtUTCTime = cancelAtUTCTime;
		}
	}



	public interface ITimerProcessor : IDisposable
	{
		int? TimerPeriod { get; }
		TaskCompletionSource<TArg> RegisterAsync<TArg>(TimeSpan delay, TaskCompletionSource<TArg> tcs = null);
		void UnRegisterAsync<TArg>(TaskCompletionSource<TArg> tcs);
	}
	
	public class TimerProcessor : ITimerProcessor
	{
		private bool Disposed { get; set; }

		private TaskCompletionSource<object> DisposeRequest { get; set; }

		private ISimpleLockUC Lock { get; } = new SpinLockUC();

		private IDictionary<object, ITimingItem> TCSToItemMapping { get; } = new Dictionary<object, ITimingItem>();
		private ISet<ITimingItem> RequestRegistration { get; } = new HashSet<ITimingItem>();
		private ISet<ITimingItem> RequestRemoval { get; } = new HashSet<ITimingItem>();
		private ISet<ITimingItem> Registered { get; } = new HashSet<ITimingItem>();
		private IList<ITimingItem> Remove { get; } = new List<ITimingItem>();

		private IRealTimeSource RealTimeSource { get; }
		private ITickGenerator TickGenerator { get; }

		public int? TimerPeriod => (TickGenerator?.PeriodAsync ?? Timing.TickGenerator.PeriodNullInt).Result;

		//public TimerProcessor()
		//{
		//}

		public TimerProcessor(IRealTimeSource realTimeSource, ITickGenerator tickGenerator)
		{
			if ((RealTimeSource = realTimeSource) == null) throw new ArgumentNullException(nameof(realTimeSource));
			if ((TickGenerator = tickGenerator) == null) throw new ArgumentNullException(nameof(tickGenerator));
			if (!TickGenerator.RegisterCallbackAsync(CallBack).Result)//waiting
			{
				throw new InvalidOperationException($"{nameof(TimerProcessor)} failed to register {nameof(ITickGenerator)} callback!");
			}
		}

		private void CallBack()
		{
			ITimingItem[] registration = Array.Empty<ITimingItem>();
			ITimingItem[] removal = Array.Empty<ITimingItem>();

			bool disposed;
		
			using (Lock.Enter())
			{
				disposed = Disposed;
				if (RequestRegistration.Count > 0)
				{
					registration = RequestRegistration.ToArray();
					RequestRegistration.Clear();
				}
				if (RequestRemoval.Count > 0)
				{
					removal = RequestRemoval.ToArray();
					RequestRemoval.Clear();
				}
			}

			if (disposed)
			{
				Registered.Clear();
				DisposeRequest?.TrySetResult(null);
				return;
			}

			for (int i = 0; i < registration.Length; i++)
			{
				Registered.Add(registration[i]);
			}

			for (int i = 0; i < removal.Length; i++)
			{
				Registered.Remove(removal[i]);
			}

			DateTime utcNow = RealTimeSource.GetUtcNow();

			foreach (ITimingItem item in Registered)
			{
				if (utcNow >= item.CancelAtUTCTime || item.IsCompleted) Remove.Add(item);
			}

			using (Lock.Enter())
			{
				for (int i = 0; i < Remove.Count; i++)
				{
					ITimingItem item = Remove[i];
					Registered.Remove(item);
					TCSToItemMapping.Remove(item.ObjectTCS);
				}
			}
		}


		public TaskCompletionSource<TArg> RegisterAsync<TArg>(TimeSpan delay, TaskCompletionSource<TArg> tcs = null)
		{
			if (delay < TimeSpan.FromMilliseconds(1) || tcs?.Task?.IsCompleted == true)
			{
				tcs = tcs ?? new TaskCompletionSource<TArg>();
				tcs.TrySetCanceled();
				return tcs;
			}
			
			ITimingItem<TArg> item = new TimingItem<TArg>(RealTimeSource.GetUtcNow() + delay, tcs);

			using (Lock.Enter())
			{
				if (Disposed)
				{
					Exception ex = new ObjectDisposedException($"{nameof(ITimerProcessor)} is disposed");
					item.TrySetException(ex);
					throw ex;
				}

				if (TCSToItemMapping.ContainsKey(item.TCS))
				{
					Exception ex = new Exception($"{nameof(ITimerProcessor)} TaskCompletionSource<{typeof(TArg).Name}> second registration");
					item.TrySetException(ex);
					throw ex;
				}

				TCSToItemMapping[item.TCS] = item;
				RequestRegistration.Add(item);

				return item.TCS;
			}
		}

		public void UnRegisterAsync<TArg>(TaskCompletionSource<TArg> tcs)
		{
			if (tcs == null) return;

			using (Lock.Enter())
			{
				if (Disposed)
				{
					Exception ex = new ObjectDisposedException($"{nameof(ITimerProcessor)} is disposed");
					tcs.TrySetException(ex);
					throw ex;
				}

				ITimingItem item;
				if (!TCSToItemMapping.TryGetValue(tcs, out item) || item == null) return;

				TCSToItemMapping.Remove(tcs);
				if (RequestRegistration.Remove(item)) return;//registration was not processed yet

				RequestRemoval.Add(item);
			}
		}

		public void Dispose()
		{
			TaskCompletionSource<object> disp;

			using (Lock.Enter())
			{
				Disposed = true;
				TCSToItemMapping.Clear();
				RequestRegistration.Clear();
				RequestRemoval.Clear();
				disp = DisposeRequest ?? new TaskCompletionSource<object>();
			}

			disp.Task.Wait();

		}
	}
}
