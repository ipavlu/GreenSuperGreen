// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Async
{
	public enum ConfigCompletionContinuation
	{
		ContinueOnCapturedContext,
		ContinueOnDefaultContext
	}

	public static class ConfigCompletionContinuationExtension
	{
		public static bool ContinueOnCapturedContext(this ConfigCompletionContinuation config) => config == ConfigCompletionContinuation.ContinueOnCapturedContext;
	}
}
