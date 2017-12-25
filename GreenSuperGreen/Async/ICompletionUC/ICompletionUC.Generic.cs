using System.Runtime.CompilerServices;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="ICompletionUC{TResult}"/> is complete .net awaitable iterface with result,
	/// it has everything the await keyword returin result needs. Making own awaitable class with result
	/// is greatly simplified with <see cref="ACompletionUC{TImplementer}"/>
	/// </summary>
	public interface ICompletionUC<out TResult> : ISimpleCompletionUC, INotifyCompletion, IUniqueID
	{
		TResult GetResult();
		ICompletionUC<TResult> GetAwaiter();
	}
}
