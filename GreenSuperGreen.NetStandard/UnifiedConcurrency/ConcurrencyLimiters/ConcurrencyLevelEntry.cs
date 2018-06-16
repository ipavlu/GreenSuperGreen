using System;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary>
	/// Represents result for using operation, to detect entry.
	/// </summary>
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
}
