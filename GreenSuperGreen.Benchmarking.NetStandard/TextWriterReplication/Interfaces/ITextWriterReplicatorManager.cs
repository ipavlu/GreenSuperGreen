using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.TextWriterReplication
{
	public interface ITextWriterReplicatorManager : ITextWriter<TextWriter>, ITextWriter, IDisposable
	{
		void AddTextWriter(TextWriter textWriter);
		void AddRangeTextWriter(IEnumerable<TextWriter> textWriters);
		void RemoveTextWriter(TextWriter textWriter);
		bool EmptyTextWriters { get; }
		void ClearTextWriters();
	}
}