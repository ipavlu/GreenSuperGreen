using System;
// ReSharper disable StaticMemberInGenericType
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.IdentifierGenerators
{
	/// <summary>
	/// Provides reasonably unique ID <see cref="System.Int32"/> and checking for equality
	/// </summary>
	public class UniqueID<TImplementer>
		:		AUniqueID<TImplementer>,
				IUniqueID,
				IEquatable<TImplementer>
		where	TImplementer
		:		class
	{
	}
}
