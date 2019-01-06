using System;
using System.IO;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.TextWriterReplication
{
	public abstract class ATextWriterInstaller : ITextWriter
	{
		private bool _disposed;
		public DisposeAncestor DisposeAncestor { get; }
		private ITextWriter Ancestor { get; } = null;
		public ITextWriterReplicatorManager ReplicatorManager { get; }

		public TextWriter TextWriterReplicator => ReplicatorManager.TextWriterReplicator;

		public TextWriter TextWriter { get; }

		protected ATextWriterInstaller(
			ITextWriter ancestor,
			ITextWriterReplicatorManager replicatorManager,
			TextWriter textWriter,
			DisposeAncestor disposeAncestor)
		{
			//TODO ancestor is not set in constructor and wont be disposed, but can break existing functionality, must be analyzed!
			ReplicatorManager = replicatorManager ?? throw new ArgumentNullException(nameof(replicatorManager));
			TextWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
			DisposeAncestor = disposeAncestor;
			//TODO Virtual member call in constructor!
			Install();
		}

		protected virtual void Install() => ReplicatorManager.AddTextWriter(TextWriter);

		protected virtual void Uninstall() => ReplicatorManager.RemoveTextWriter(TextWriter);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				Uninstall();
				if (DisposeAncestor == DisposeAncestor.Yes) Ancestor?.Dispose();
				//TODO ancestor is not set in constructor and wont be disposed, but can break existing functionality, must be analyzed!
			}
			_disposed = true;
			//base.Dispose(disposing);
		}

		public virtual ITextWriter Install(TextWriter textWriter, DisposeAncestor disposeAncestor) => new TextWriterInstaller(this, ReplicatorManager, textWriter, disposeAncestor);

		public ITextWriter<TInstall> Install<TInstall>(TInstall textWriter, DisposeAncestor disposeAncestor) where TInstall : TextWriter
		=> new TextWriterInstaller<TInstall>(this, ReplicatorManager, textWriter, disposeAncestor)
		;

		~ATextWriterInstaller() => Dispose(false);
	}
}