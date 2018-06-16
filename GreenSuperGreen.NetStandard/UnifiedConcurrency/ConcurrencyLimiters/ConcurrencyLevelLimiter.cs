using System;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> Helps limit number of concurrent threads without hard locking </summary>
	public interface IConcurrencyLevelCounter
	{
		int MaxConcurrencyLevel { get; }
		int ConcurrencyLevel { get; }
		ConcurrencyLevelEntry TryEnter();
	}

	/// <summary> Helps limit number of concurrent threads without hard locking </summary>
	public class ConcurrencyLevelLimiter : IConcurrencyLevelCounter
	{
		private ILockUC Lock { get; } = new SpinLockUC();
		private int ConcurrencyLevelValue { get; set; }
		public int ConcurrencyLevel { get { using (Lock.Enter()) return ConcurrencyLevelValue; } }
		public int MaxConcurrencyLevel { get; }
		private Action ActionOnExit { get; }

		public ConcurrencyLevelLimiter(int maxConcurrency)
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
				if (ConcurrencyLevelValue >= MaxConcurrencyLevel) return ConcurrencyLevelEntry.NoEntry;
				++ConcurrencyLevelValue;
				return new ConcurrencyLevelEntry(ActionOnExit);
			}
		}
	}
}
