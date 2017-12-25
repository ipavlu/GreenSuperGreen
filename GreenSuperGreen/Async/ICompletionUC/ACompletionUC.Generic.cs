using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GreenSuperGreen.IdentifierGenerators;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace GreenSuperGreen.Async
{
	/// <summary>
	/// Please note that <see cref="ACompletionUC{TImplementer, TResult}"/> implements
	/// <see cref="ICompletionUC{TResult}"/> interface, the complete .net awaitable iterface,
	/// it has everything the await keyword with result needs. Making own awaitable class
	/// is greatly simplified.
	/// </summary>
	/// <typeparam name="TImplementer">The class inheriting ACompletionUC</typeparam>
	/// <typeparam name="TResult"></typeparam>
	public abstract class ACompletionUC<TImplementer, TResult>
		:	AUniqueID<TImplementer>,
			ICompletionUC<TResult>,
			ISimpleCompletionUC,
			INotifyCompletion,
			IUniqueID
		where TImplementer : class
	{
		/// <summary>
		/// Think twice before you use <see cref="_tcs"/>,
		/// giving public access to the <see cref="TaskCompletionSource{TResult}.Task"/>
		/// allows awaiting on the notification Task, effectivelly overriding
		/// awaiting on interface <see cref="ICompletionUC{TResult}"/>, possibly
		/// blocking forever...
		/// </summary>
		private readonly TaskCompletionSource<TResult> _tcs;

		/// <summary>
		/// <see cref="TaskCompletionSource{TResult}"/> created with
		/// <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/>
		/// </summary>
		protected ACompletionUC()
		{
			_tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		}

		/// <summary>
		/// <see cref="TaskCompletionSource{TResult}"/> created with
		/// <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/> if request in
		/// <see cref="taskCreationOptions"/>
		/// </summary>
		protected ACompletionUC(TaskCreationOptions taskCreationOptions)
		{
			_tcs = new TaskCompletionSource<TResult>(taskCreationOptions & TaskCreationOptions.RunContinuationsAsynchronously);
		}

		/// <summary>
		/// True when awaitable operation is completed, visible in interfaces
		/// <see cref="ICompletionUC{TResult}"/>, <see cref="ISimpleCompletionUC"/>, <see cref="INotifyCompletion"/> 
		/// </summary>
		public virtual bool IsCompleted => _tcs.Task.GetAwaiter().IsCompleted;

		/// <summary>
		/// GetResult is actually synchronously waiting on completion of the awaitable operation if it is not yet completed.
		/// </summary>
		public virtual TResult GetResult() => _tcs.Task.GetAwaiter().GetResult();

		/// <summary> Called by compiler services after await keyword </summary>
		public virtual ICompletionUC<TResult> GetAwaiter() => this;

		/// <summary>
		/// Compiler services first calling GetAwaiter().
		/// The returned awaiter must implement OnCompleted method, but it does not have to be called by CompilerServices!
		/// If the work based on IsCompleted is already completed, it can skip this step and execute continuation right away.
		///	DEPENDING ON COMPILER SERVICES ALWAYS CALLING THIS METHOD IS ILL ADVISED.
		/// </summary>
		public virtual void OnCompleted(Action continuation) => _tcs.Task.GetAwaiter().OnCompleted(continuation);

		/// <summary>
		/// SetCompletion is protected, invisible to interfaces,
		/// activated by inheritor of awaitable operation when operation is completed,
		/// signalling that continuation below await keyword can be executed.
		/// </summary>
		protected bool SetCompletion(TResult rslt) => _tcs.TrySetResult(rslt);

		/// <summary>
		/// SetException is protected, invisible to interfaces,
		/// activated by inheritor of awaitable operation when operation takes wrong turn,
		/// exception will be redirected to await context.
		/// </summary>
		protected bool SetException(Exception ex) => _tcs.TrySetException(ex);
	}
}
