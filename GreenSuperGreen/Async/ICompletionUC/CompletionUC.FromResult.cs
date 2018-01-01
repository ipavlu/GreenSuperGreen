using System.Runtime.CompilerServices;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local


namespace GreenSuperGreen.Async
{
	public static partial class CompletionUC
	{
		private class GenericCompletedCompletionUC<TResult>
		: CompletedCompletionUC<GenericCompletedCompletionUC<TResult>, TResult>,
			ICompletionUC<TResult>,
			ISimpleCompletionUC,
			INotifyCompletion,
			ICriticalNotifyCompletion,
			IUniqueID
		{
			public GenericCompletedCompletionUC(TResult result = default(TResult)) : base(result) { }
		}

		/// <summary>
		/// This provides a static completed <see cref="ICompletionUC{TResult}"/>, exists for simplification and performance reasons.
		/// Static singleton.
		/// </summary>
		public static ICompletionUC<TResult> FromResult<TResult>(TResult result = default(TResult))
		=> new GenericCompletedCompletionUC<TResult>(result)
		;

		/// <summary>
		/// This servers as static completed , exists for simplification and performance reasons.
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// These are static singletons too, but reference wise they are different for each <see cref="TImplementer"/>.
		/// </summary>
		public static ICompletionUC<TResult> FromResult<TImplementer,TResult>(TResult result = default(TResult))
		where TImplementer : class
		=> new CompletedCompletionUC<TImplementer, TResult>(result)
		;
	}
}
