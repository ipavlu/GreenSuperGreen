using System.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable PartialTypeWithSinglePart

namespace GreenSuperGreen.Contexts
{
	public
	static
	partial
	class
	ExecutionContextExtensions
	{
		/// <summary>
		/// When delegate <see cref="ContextCallback"/> is not null,  then attempting to invoke
		/// the delegate on provided <see cref="ExecutionContext"/> if it is not null. Otherwise
		/// trying to invoke the delegate on current <see cref="ExecutionContext"/>.
		/// </summary>
		public
		static
		void
		RunNullSafe(this	ExecutionContext executionContext,
							ContextCallback callBack,
							object arg)
		{
			if (callBack == null) return;
			if (executionContext != null)
			{
				ExecutionContext.Run(executionContext, callBack, arg);
				return;
			}
			callBack(arg);
		}
	}
}