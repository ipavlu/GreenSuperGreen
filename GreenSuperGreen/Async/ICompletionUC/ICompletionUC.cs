using System.Runtime.CompilerServices;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace


namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="ICompletionUC"/> is complete .net awaitable iterface,
	/// it has everything the await keyword needs. Making own awaitable class
	/// is greatly simplified with <see cref="ACompletionUC{TImplementer}"/>.
	/// </summary>
	public interface ICompletionUC : ISimpleCompletionUC, INotifyCompletion, IUniqueID
	{
		void GetResult();
		ICompletionUC GetAwaiter();
	}
}
