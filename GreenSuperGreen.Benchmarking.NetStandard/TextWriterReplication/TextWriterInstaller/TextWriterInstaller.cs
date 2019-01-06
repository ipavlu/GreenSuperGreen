using System.IO;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.TextWriterReplication
{
	public class TextWriterInstaller : ATextWriterInstaller
	{
		private bool _disposed;

		public TextWriterInstaller(ITextWriter ancestor, ITextWriterReplicatorManager replicatorManager, TextWriter textWriter, DisposeAncestor disposeAncestor)
			: base(ancestor, replicatorManager, textWriter, disposeAncestor)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
			}

			_disposed = true;
			base.Dispose(disposing);
		}

		~TextWriterInstaller() => Dispose(false);
	}
}