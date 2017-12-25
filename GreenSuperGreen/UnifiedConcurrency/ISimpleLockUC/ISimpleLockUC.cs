// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.UnifiedConcurrency
{
	public interface ISimpleLockUC : ISimpleLockUC<EntryBlockUC, object>
	{
	}

	public interface ISimpleLockUC<out TEntryBlock, TArg> where TEntryBlock : IEntryBlockUC<TArg>
	{
		SyncPrimitiveCapabilityUC Capability { get; }

		TEntryBlock Enter();
		TEntryBlock TryEnter();
		TEntryBlock TryEnter(int milliseconds);
	}
}
