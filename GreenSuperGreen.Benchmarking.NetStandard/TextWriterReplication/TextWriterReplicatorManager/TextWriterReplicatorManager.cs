using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using GreenSuperGreen.Collections.Concurrent;

namespace GreenSuperGreen.TextWriterReplication
{
	/// <summary>
	/// The <see cref="TextWriterReplicatorManager"/> allows to replicate data to other <see cref="TextWriter"/> targets
	/// </summary>
	public sealed partial class TextWriterReplicatorManager
		:	TextWriter,
			ITextWriterReplicatorManager,
			ITextWriter<TextWriter>
	{
		private bool disposed = false;

		private IConcurrentDistinctInOrderCollection<TextWriter> TextWriters { get; } = new ConcurrentDistinctInOrderCollection<TextWriter>();

		public DisposeAncestor DisposeAncestor { get; } = DisposeAncestor.No;
		public ITextWriterReplicatorManager ReplicatorManager => this;
		public TextWriter TextWriterReplicator => this;
		public TextWriter TextWriter => this;
		public TextWriter TextWriterOfT => this;

		public TextWriterReplicatorManager() : base() { }

		public TextWriterReplicatorManager(IFormatProvider formatProvider) : base(formatProvider)
		{
			if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));
		}

		public TextWriterReplicatorManager(TextWriter textWriter)
			: this(new[] { textWriter }) { }

		public TextWriterReplicatorManager(IFormatProvider formatProvider, TextWriter textWriter)
			: this(formatProvider, new[] { textWriter }) { }

		public TextWriterReplicatorManager(IEnumerable<TextWriter> textWriters) : base()
		{
			TextWriters.AddRange(textWriters);
			if (textWriters == null) throw new ArgumentNullException(nameof(textWriters));
			if (TextWriters.IsEmpty) throw new Exception($"{nameof(textWriters)} are empty");
		}

		public TextWriterReplicatorManager(IFormatProvider formatProvider, IEnumerable<TextWriter> textWriters)
			: base(formatProvider)
		{
			TextWriters.AddRange(textWriters);
			if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));
			if (textWriters == null) throw new ArgumentNullException(nameof(textWriters));
			if (TextWriters.IsEmpty) throw new Exception($"{nameof(textWriters)} are empty");
		}


		public override Encoding Encoding => TextWriters?.DistinctInOrderArray.FirstOrDefault()?.Encoding ?? Encoding.Unicode;

		public override string NewLine
		{
			get => TextWriters?.DistinctInOrderArray.FirstOrDefault()?.NewLine ?? Environment.NewLine;
			set
			{
				var textWriter = TextWriters.DistinctInOrderArray;
				for (int i = 0; i < textWriter.Length; ++i)
				{
					textWriter[i].NewLine = value;
				}
			}
		}

		public void AddTextWriter(TextWriter textWriter) => TextWriters.Add(textWriter);
		public void AddRangeTextWriter(IEnumerable<TextWriter> textWriters) => TextWriters.AddRange(textWriters);
		public void RemoveTextWriter(TextWriter textWriter) => TextWriters.Remove(textWriter);
		public bool EmptyTextWriters => TextWriters.IsEmpty;

		public void ClearTextWriters() => TextWriters.Clear();

		public override void Close() => Dispose(true);

		public override void Flush()
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Flush();
		}

		public override async Task FlushAsync()
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].FlushAsync();
		}

		public override void Write(ulong value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(uint value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(string format, params object[] arg)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(format, arg);
		}

		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(format, arg0, arg1, arg2);
		}

		public override void Write(string format, object arg0, object arg1)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(format, arg0, arg1);
		}

		public override void Write(string format, object arg0)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(format, arg0);
		}

		public override void Write(string value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(object value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(long value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(int value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(double value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(decimal value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(char[] buffer, int index, int count)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(buffer, index, count);
		}

		public override void Write(char[] buffer)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(buffer);
		}

		public override void Write(char value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(bool value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override void Write(float value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Write(value);
		}

		public override async Task WriteAsync(string value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteAsync(value);
		}

		public new async Task WriteAsync(char[] buffer)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteAsync(buffer);
		}

		public async override Task WriteAsync(char value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteAsync(value);
		}
		
		public async override Task WriteAsync(char[] buffer, int index, int count)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteAsync(buffer, index, count);
		}

		public override void WriteLine(string format, object arg0)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(format, arg0);
		}

		public override void WriteLine(ulong value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(uint value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(string format, params object[] arg)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(format, arg);
		}

		public override void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(format, arg0, arg1, arg2);
		}

		public override void WriteLine(string format, object arg0, object arg1)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(format, arg0, arg1);
		}

		public override void WriteLine(string value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(float value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine()
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine();
		}

		public override void WriteLine(long value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(int value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(double value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(decimal value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(buffer, index, count);
		}

		public override void WriteLine(char[] buffer)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(buffer);
		}

		public override void WriteLine(char value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(bool value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override void WriteLine(object value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) textWriter[i].WriteLine(value);
		}

		public override async Task WriteLineAsync()
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteLineAsync();
		}

		public override async Task WriteLineAsync(char value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteLineAsync(value);
		}

		public new async Task WriteLineAsync(char[] buffer)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteLineAsync(buffer);
		}

		public override async Task WriteLineAsync(char[] buffer, int index, int count)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteLineAsync(buffer, index, count);
		}

		public override async Task WriteLineAsync(string value)
		{
			var textWriter = TextWriters.DistinctInOrderArray;
			for (int i = 0; i < textWriter.Length; ++i) await textWriter[i].WriteLineAsync(value);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				var textWriter = TextWriters.DistinctInOrderArray;
				for (int i = 0; i < textWriter.Length; ++i) textWriter[i].Dispose();
				TextWriters.Clear();
			}

			disposed = true;
			base.Dispose(disposing);
		}

		public ITextWriter<TInstall> Install<TInstall>(TInstall textWriter, DisposeAncestor disposeAncestor) where TInstall : TextWriter
		=> new TextWriterInstaller<TInstall>(this, this, textWriter, disposeAncestor)
		;

		public ITextWriter Install(TextWriter textWriter, DisposeAncestor disposeAncestor) => new TextWriterInstaller(this, this, textWriter, disposeAncestor);

		~TextWriterReplicatorManager() => Dispose(false);
	}
}