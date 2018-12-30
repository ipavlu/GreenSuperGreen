using System.IO;

namespace GreenSuperGreen.TextWriterReplication
{
	public class TextWriterInstaller<T> : ATextWriterInstaller<T> where T: TextWriter
	{
		public TextWriterInstaller(ITextWriter ancestor, ITextWriterReplicatorManager replicatorManager, T textWriterOfT, DisposeAncestor disposeAncestor)
			: base(ancestor, replicatorManager, textWriterOfT, disposeAncestor)
		{
		}

		~TextWriterInstaller() => Dispose(false);
	}
}