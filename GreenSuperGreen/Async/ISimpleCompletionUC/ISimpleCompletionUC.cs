using System.Runtime.CompilerServices;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	public interface ISimpleCompletionUC : INotifyCompletion, ICriticalNotifyCompletion
	{
		bool IsCompleted { get; }
	}
}
