using System.Runtime.CompilerServices;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="ICompletionUC"/> is complete .net awaitable interface,  it has everything the await keyword needs.
	/// </summary>
	public interface ICompletionUC : ISimpleCompletionUC, INotifyCompletion, ICriticalNotifyCompletion, IUniqueID
	{
		void GetResult();
		ICompletionUC GetAwaiter();
	}
}
