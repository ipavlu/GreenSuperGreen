// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace GreenSuperGreen.UnifiedConcurrency
{
	public interface IAsyncLockUC : IAsyncLockUC<AsyncEntryBlockUC> { }

	public interface IAsyncLockUC<out TAsyncEntryBlockUC>
	{
		SyncPrimitiveCapabilityUC Capability { get; }

		TAsyncEntryBlockUC Enter();
		TAsyncEntryBlockUC TryEnter();
		TAsyncEntryBlockUC TryEnter(int milliseconds);
	}
}
