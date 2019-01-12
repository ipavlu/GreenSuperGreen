using System;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
 
namespace GreenSuperGreen.UnifiedConcurrency
{
	public struct EntryBlockUC
	:	IEntryBlockUC,
		IDisposable,
		IEquatable<EntryBlockUC>
	{
		public static EntryBlockUC RefusedEntry { get; } = new EntryBlockUC(EntryTypeUC.None, null);

		private IEntryCompletionUC EntryCompletion { get; }

		public EntryTypeUC EntryTypeUC { get; }
		public bool HasEntry => EntryTypeUC != EntryTypeUC.None;

		public object EntryToken => EntryCompletion?.EntryToken;

		public EntryBlockUC(EntryTypeUC entryTypeUC, IEntryCompletionUC entryCompletion)
		{
			if (entryCompletion == null && entryTypeUC != EntryTypeUC.None) throw new ArgumentNullException(nameof(entryCompletion));
			EntryCompletion = entryCompletion;
			EntryTypeUC = entryTypeUC;
		}

		public void Dispose() => EntryCompletion?.Complete();

		public override int GetHashCode()
		{
			unchecked
			{
				return ((EntryCompletion?.GetHashCode() ?? 0) * 397) ^ (int)EntryTypeUC;
			}
		}

		public bool Equals(EntryBlockUC other) => Equals(EntryCompletion, other.EntryCompletion) && EntryTypeUC == other.EntryTypeUC;
		public override bool Equals(object obj) => obj is EntryBlockUC entry && Equals(entry);

		public static bool operator ==(EntryBlockUC a, EntryBlockUC b) => a.Equals(b);
		public static bool operator !=(EntryBlockUC a, EntryBlockUC b) => !a.Equals(b);
	}
}
