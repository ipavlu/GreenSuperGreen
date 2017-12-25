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
		private class GenericExceptionCompletionUC<TResult>
		: ExceptionCompletionUC<ExceptionCompletionUC, TResult>,
			ICompletionUC<TResult>,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		{
			public GenericExceptionCompletionUC(Exception exception) : base(exception) { }
		}

		/// <summary>
		/// This provides <see cref="ICompletionUC{TResult}"/> which will throw requested exception upon await, for simplification and performance reasons.
		/// </summary>
		public static ICompletionUC<TResult> FromExceptionWithResult<TResult>(Exception exception) => new GenericExceptionCompletionUC<TResult>(exception);

		/// <summary>
		/// This provides <see cref="ICompletionUC{TResult}"/> which will throw exception upon await, for simplification and performance reasons.
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// </summary>
		public static ICompletionUC<TResult> FromException<TImplementer, TResult>(Exception exception)
		where TImplementer : class
		=> new ExceptionCompletionUC<TImplementer, TResult>(exception)
		;
	}
}
