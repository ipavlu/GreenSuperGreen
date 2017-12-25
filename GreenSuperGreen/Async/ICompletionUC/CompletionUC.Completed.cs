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
		private class CompletedCompletionUC
		: CompletedCompletionUC<CompletedCompletionUC>,
			ICompletionUC,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		{
		}

		/// <summary>
		/// This servers as static completed <see cref="ICompletionUC"/>, exists for simplification and performance reasons.
		/// Static singleton.
		/// </summary>
		public static ICompletionUC Completed() => CompletedCompletionUC.Completed;

		/// <summary>
		/// This servers as static completed <see cref="ICompletionUC"/>, exists for simplification and performance reasons.
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// These are static singletons too, but reference wise they are different for each <see cref="TImplementer"/>.
		/// </summary>
		public static ICompletionUC Completed<TImplementer>()
		where TImplementer : class
		=> CompletedCompletionUC<TImplementer>.Completed
		;
	}
}
