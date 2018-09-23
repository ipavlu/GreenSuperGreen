// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public enum SeqContinuationUC
	{
		/// <summary> Captured current context in production code </summary>
		OnCapturedContext,

		/// <summary> On Test context, executing synchronously from test code. </summary>
		OnCallersContext,

		/// <summary>
		/// <para/> On new context
		/// <para/> scheduled on <see cref="System.Threading.Tasks.TaskScheduler.Default"/>
		/// <para/> that goes usually to <see cref="System.Threading.ThreadPool"/> unless it runs in special environment!
		/// </summary>
		OnNewContext
	}
}
