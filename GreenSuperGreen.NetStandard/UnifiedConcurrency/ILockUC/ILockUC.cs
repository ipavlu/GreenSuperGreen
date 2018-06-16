using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.UnifiedConcurrency
{
	#pragma warning disable 618 //disable obsolete warning
		public interface ILockUC : ILockUC<EntryBlockUC, object>, ISimpleLockUC<EntryBlockUC, object>, ISimpleLockUC
		{
		}
	#pragma warning restore 618 //enable obsolete warning


	[Obsolete("Scheduled for removal in version 2.0.0")]
	public interface ISimpleLockUC : ISimpleLockUC<EntryBlockUC, object>
	{
	}

	#pragma warning disable 618 //disable obsolete warning
		public interface ILockUC<out TEntryBlock, TArg> : ISimpleLockUC<TEntryBlock, TArg> where TEntryBlock : IEntryBlockUC<TArg>
		{
			new SyncPrimitiveCapabilityUC Capability { get; }

			new TEntryBlock Enter();
			new TEntryBlock TryEnter();
			new TEntryBlock TryEnter(int milliseconds);
		}
	#pragma warning restore 618 //enable obsolete warning

	[Obsolete("Scheduled for removal in version 2.0.0")]
	public interface ISimpleLockUC<out TEntryBlock, TArg> where TEntryBlock : IEntryBlockUC<TArg>
	{
		SyncPrimitiveCapabilityUC Capability { get; }

		TEntryBlock Enter();
		TEntryBlock TryEnter();
		TEntryBlock TryEnter(int milliseconds);
	}
}
