using System;
using System.Threading;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

namespace GreenSuperGreen.IdentifierGenerators
{
	public abstract class AUniqueID<TImplementer>
		:		IUniqueID,
				IEquatable<TImplementer>
		where	TImplementer
		:		class
	{
		/// <summary> 
		/// The static field is unique per type <see cref="TImplementer"/>, it is used intentionaly.
		/// </summary>
		private static int _lastID = -1;

		/// <summary>
		/// If you know who implements this, you can get next id in sequence.
		/// As id's are not used for counting, ordering, just reasonable indentification,
		/// then it does not matter if somebody gets next id in the sequence.
		/// </summary>
		public static int GenerateID() => Interlocked.Increment(ref _lastID);

		/// <summary>
		/// Assigning reasonably unique id to instances of the <see cref="TImplementer"/>,
		/// <see cref="AUniqueID{TImplementer}"/>
		/// </summary>
		public virtual int UniqueID { get; } = GenerateID();

		
		/// <summary> Based on <see cref="UniqueID"/> </summary>
		public override int GetHashCode() => UniqueID;

		/// <summary>
		/// Based on <see cref="UniqueID"/>.
		/// For non null and same id's, <see cref="object.ReferenceEquals"/> is used
		/// </summary>
		public virtual bool Equals(TImplementer other)
		{
			return
			other != null &&
			other.GetHashCode() == this.GetHashCode() &&
			ReferenceEquals(other, this)
			;
		}

		/// <summary>
		/// Based on <see cref="UniqueID"/>.
		/// For non null and same id's, <see cref="object.ReferenceEquals"/> is used
		/// </summary>
		public override bool Equals(object obj) => Equals(obj as TImplementer);
	}
}
