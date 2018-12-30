using System.IO;

namespace GreenSuperGreen.TextWriterReplication
{
	public class TextWriterInstaller : ATextWriterInstaller
	{
		private bool disposed = false;

		public TextWriterInstaller(ITextWriter ancestor, ITextWriterReplicatorManager replicatorManager, TextWriter textWriter, DisposeAncestor disposeAncestor)
			: base(ancestor, replicatorManager, textWriter, disposeAncestor)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
			}

			disposed = true;
			base.Dispose(disposing);
		}

		~TextWriterInstaller() => Dispose(false);
	}
}