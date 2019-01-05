using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedTypeParameter

namespace GreenSuperGreen.UnifiedConcurrency
{
	/// <summary> Defines what kind access into protected block is requested </summary>
	public enum EntryTypeUC
	{
		/// <summary> Has no access</summary>
		None,
		/// <summary> One at the time </summary>
		Exclusive,
		/// <summary> One or many at the time </summary>
		Concurrent
	}

	public static class EntryAccessUCExtension
	{
		[Obsolete("In 2.0.0 will be removed, use IsExclusive")]
		// ReSharper disable once IdentifierTypo
		public static bool IsExlusive(this EntryTypeUC entryType) => entryType.IsExclusive();
		public static bool IsExclusive(this EntryTypeUC entryType) => entryType == EntryTypeUC.Exclusive;

		[Obsolete("In 2.0.0 will be removed, use IsExclusive")]
		// ReSharper disable once IdentifierTypo
		public static bool IsExlusive(this EntryTypeUC? entryType) => entryType.IsExclusive();
		public static bool IsExclusive(this EntryTypeUC? entryType) => entryType.HasValue && entryType.Value == EntryTypeUC.Exclusive;

		public static bool IsConcurrent(this EntryTypeUC entryType) => entryType == EntryTypeUC.Concurrent;
		public static bool IsConcurrent(this EntryTypeUC? entryType) => entryType.HasValue && entryType.Value == EntryTypeUC.Concurrent;
	}
}
