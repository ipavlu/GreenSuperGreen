using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="CompletionFromTaskUC{TImplementer}"/> implements
	/// <see cref="ICompletionUC"/> interface, the complete .net awaitable iterface,
	/// it has everything the await keyword needs. Here created from <see cref="Task"/>
	/// </summary>
	/// <typeparam name="TImplementer">The class inheriting <see cref="CompletionFromTaskUC{TImplementer}"/></typeparam>
	public class CompletionFromTaskUC<TImplementer>
		:	AUniqueID<TImplementer>,
			ICompletionUC,
			ISimpleCompletionUC,
			INotifyCompletion,
			ICriticalNotifyCompletion,
			IUniqueID
		where TImplementer : class 
	{
		private Task TaskResult { get; }

		public CompletionFromTaskUC(Task task)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));
			TaskResult = task;
		}

		/// <summary>
		/// True when awaitable operation is completed, visible in interfaces
		/// <see cref="ICompletionUC"/>, <see cref="ISimpleCompletionUC"/>, <see cref="INotifyCompletion"/> 
		/// </summary>
		public virtual bool IsCompleted => TaskResult.GetAwaiter().IsCompleted;

		/// <summary>
		/// GetResult is actually synchronously waiting on completion of the awaitable operation if it is not yet completed.
		/// </summary>
		public virtual void GetResult() => TaskResult.GetAwaiter().GetResult();

		/// <summary> Called by compiler services after await keyword </summary>
		public virtual ICompletionUC GetAwaiter() => this;

		/// <summary>
		/// Compiler services first calling GetAwaiter().
		/// The returned awaiter must implement OnCompleted method, but it does not have to be called by CompilerServices!
		/// If the work based on IsCompleted is already completed, it can skip this step and execute continuation right away.
		///	DEPENDING ON COMPILER SERVICES ALWAYS CALLING THIS METHOD IS ILL ADVISED.
		/// </summary>
		public virtual void OnCompleted(Action continuation) => TaskResult.GetAwaiter().OnCompleted(continuation);

		[SecurityCritical]
		public virtual void UnsafeOnCompleted(Action continuation) => TaskResult.GetAwaiter().OnCompleted(continuation);
	}
}
