using System;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.UnifiedConcurrency
{
	public interface IEntryCompletionUC : IEntryCompletionUC<object>
	{
	}

	public interface IEntryCompletionUC<out TEntryToken>
	{
		TEntryToken EntryToken { get; }
		void Complete();
	}

	public class EntryCompletionUC : IEntryCompletionUC
	{
		private Action DisposeAction { get; }
		public object EntryToken { get; }

		public EntryCompletionUC(Action disposeAction, object entryToken = null)
		{
			if ((DisposeAction = disposeAction) == null) throw new ArgumentNullException(nameof(disposeAction));
			EntryToken = entryToken;
		}

		public void Complete() => DisposeAction();
	}
}
