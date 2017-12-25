using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
		private class CompletionFromTaskUC
		: CompletionFromTaskUC<CompletionFromTaskUC>,
			ICompletionUC,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		{
			public CompletionFromTaskUC(Task task) : base(task) { }
		}

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> from <see cref="Task"/>.
		/// </summary>
		public static ICompletionUC FromTask(Task task) => new CompletionFromTaskUC(task);

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> from <see cref="Task"/>
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// </summary>
		public static ICompletionUC FromTask<TImplementer>(Task task)
		where TImplementer : class
		=> new CompletionFromTaskUC<TImplementer>(task)
		;

		private class GenericCompletionFromTaskUC<TResult>
		:	CompletionFromTaskUC<GenericCompletionFromTaskUC<TResult>, TResult>,
			ICompletionUC<TResult>,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		{
			public GenericCompletionFromTaskUC(Task<TResult> task) : base(task) { }
		}

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> from <see cref="Task"/>.
		/// </summary>
		public static ICompletionUC<TResult> FromTask<TResult>(Task<TResult> task) => new GenericCompletionFromTaskUC<TResult>(task);

		/// <summary>
		/// This provides <see cref="ICompletionUC"/> from <see cref="Task"/>
		/// It is generic based on <see cref="TImplementer"/>, allowing to define type specific versions.
		/// </summary>
		public static ICompletionUC<TResult> FromTask<TImplementer,TResult>(Task<TResult> task)
		where TImplementer : class
		=> new CompletionFromTaskUC<TImplementer, TResult>(task)
		;



	}
}
