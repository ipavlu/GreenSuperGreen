using System;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.UnifiedConcurrency
{
	public interface IEntryBlockUC : IEntryBlockUC<object>, IDisposable
	{
	}

	public interface IEntryBlockUC<out TEntryToken> : IDisposable
	{
		EntryTypeUC EntryTypeUC { get; }
		bool HasEntry { get; }
		TEntryToken EntryToken { get; }
	}
}
