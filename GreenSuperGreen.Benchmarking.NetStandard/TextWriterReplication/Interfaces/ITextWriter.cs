using System;
using System.IO;

namespace GreenSuperGreen.TextWriterReplication
{
	public interface ITextWriter : IDisposable
	{
		DisposeAncestor DisposeAncestor { get; }
		ITextWriterReplicatorManager ReplicatorManager { get; }
		TextWriter TextWriterReplicator { get; }
		TextWriter TextWriter { get; }
		ITextWriter Install(TextWriter textWriter, DisposeAncestor disposeAncestor);
		ITextWriter<TInstall> Install<TInstall>(TInstall textWriter, DisposeAncestor disposeAncestor) where TInstall : TextWriter;
	}

	public interface ITextWriter<T> : ITextWriter, IDisposable
		where T : TextWriter
	{
		T TextWriterOfT { get; }
	}
}