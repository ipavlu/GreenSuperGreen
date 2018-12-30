using System.IO;

namespace GreenSuperGreen.TextWriterReplication
{
	public static class ITextWriterExtension
	{
		public static ITextWriter InstallConsole(this ITextWriter textWriter, DisposeAncestor disposeAncestor = DisposeAncestor.No) => new ConsoleTextWriterInstaller(textWriter, textWriter.ReplicatorManager, disposeAncestor);

		public static ITextWriter<TInstall> InstallInto<TInstall>(this TInstall installTextWriter, ITextWriter textWriter, DisposeAncestor disposeAncestor)
			where TInstall : TextWriter
			=> new TextWriterInstaller<TInstall>(textWriter, textWriter.ReplicatorManager, installTextWriter, disposeAncestor)
		;
	}
}