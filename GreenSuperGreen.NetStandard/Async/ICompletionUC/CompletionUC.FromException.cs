using System;
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
		private class ExceptionCompletionUC
		: ExceptionCompletionUC<ExceptionCompletionUC>,
			ICompletionUC,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		{
			public ExceptionCompletionUC(Exception exception) : base(exception) { }
		}

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> which will throw requested exception upon await, for simplification and performance reasons.
		/// </summary>
		public static ICompletionUC FromException(Exception exception) => new ExceptionCompletionUC(exception);

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> which will throw exception upon await, for simplification and performance reasons.
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// </summary>
		public static ICompletionUC FromException<TImplementer>(Exception exception)
		where TImplementer : class
		=> new ExceptionCompletionUC<TImplementer>(exception)
		;
	}
}
