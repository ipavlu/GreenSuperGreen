using System.IO;

namespace GreenSuperGreen.TextWriterReplication
{
	public abstract class ATextWriterInstaller<T> : ATextWriterInstaller, ITextWriter<T> where T : TextWriter
	{
		public T TextWriterOfT { get; }

		protected ATextWriterInstaller(
			ITextWriter ancestor,
			ITextWriterReplicatorManager replicatorManager,
			T textWriterOfT,
			DisposeAncestor disposeAncestor)
			: base(ancestor, replicatorManager, textWriterOfT, disposeAncestor)
		{
			TextWriterOfT = textWriterOfT;
		}

		~ATextWriterInstaller() => Dispose(false);
	}
}