using System.Runtime.CompilerServices;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="ICompletionUC{TResult}"/> is complete .net awaitable interface with result, it has everything the await keyword returning result needs.
	/// </summary>
	public interface ICompletionUC<out TResult> : ISimpleCompletionUC, INotifyCompletion, ICriticalNotifyCompletion, IUniqueID
	{
		TResult GetResult();
		ICompletionUC<TResult> GetAwaiter();
	}
}
