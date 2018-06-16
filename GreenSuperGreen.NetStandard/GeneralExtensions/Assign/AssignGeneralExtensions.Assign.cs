using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
// ReSharper disable RedundantAssignment

namespace GreenSuperGreen
{
	public static class AssignGeneralExtensions
	{
		public static TAssign AssignOut<TAssign>(this TAssign assign, out TAssign assignOut)
		{
			assignOut = assign;
			return assign;
		}

		public static TAssign AssignRef<TAssign>(this TAssign assign, ref TAssign assignRef)
		{
			assignRef = assign;
			return assign;
		}

		public static TAssign Assign<TAssign>(this TAssign assign, Action<TAssign> assignAction = null)
		{
			assignAction?.Invoke(assign);
			return assign;
		}

		public static IEnumerable<TAssign> Assign<TAssign>(this IEnumerable<TAssign> ienumerable, Action<TAssign> assignAction = null)
		{
			if (ienumerable == null) throw new ArgumentNullException(nameof(ienumerable));

			using (IEnumerator<TAssign> enumerator = ienumerable.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					assignAction?.Invoke(enumerator.Current);
					yield return enumerator.Current;
				}
			}
		}
	}
}
