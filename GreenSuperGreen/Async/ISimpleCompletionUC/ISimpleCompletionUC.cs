using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	public interface ISimpleCompletionUC : INotifyCompletion
	{
		bool IsCompleted { get; }
	}
}
