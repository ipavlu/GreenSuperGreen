using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Benchmarking
{
	public interface IThreadGroupIndex
	{
		int Id { get; }
	}

	public class ThreadGroupIndex : IThreadGroupIndex, IEquatable<ThreadGroupIndex>
	{
		public static IThreadGroupIndex New(int id) => new ThreadGroupIndex(id);

		public int Id { get; }

		public ThreadGroupIndex(int threadGroupIndex) => Id = threadGroupIndex;

		public override bool Equals(object obj) => Equals(obj as ThreadGroupIndex);

		public bool Equals(ThreadGroupIndex other) => other != null && Id == other.Id;

		public override int GetHashCode() => Id;

		public override string ToString() => $"[GrpId:{Id}]";

		public static bool operator ==(ThreadGroupIndex index1, ThreadGroupIndex index2)
		=> EqualityComparer<ThreadGroupIndex>.Default.Equals(index1, index2)
		;

		public static
		bool operator !=(ThreadGroupIndex index1, ThreadGroupIndex index2) => !(index1 == index2);
	}
}