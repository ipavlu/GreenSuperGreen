using System;
using System.ComponentModel;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues
{
	internal
	class
	PriorityEnumCheckerUC<TPrioritySelectorEnum>
	where TPrioritySelectorEnum : struct
	{
		public static bool IsNotEnum { get; } = !typeof(TPrioritySelectorEnum).IsEnum;
		public static bool IsFlaggedEnum { get; } = Attribute.IsDefined(typeof(TPrioritySelectorEnum), typeof(FlagsAttribute));

		public static void TestEnum(string typename)
		{
			typename = typename ?? "UNSPECIFIED";
			if (IsNotEnum) throw new InvalidEnumArgumentException($"{typeof(TPrioritySelectorEnum).Name} not an Enum!");
			if (IsFlaggedEnum) throw new InvalidEnumArgumentException($"{typeof(TPrioritySelectorEnum).Name} is flagged Enum! Flagged enums are not supported for {typename} as multiple combinations are not supported.");
		}

		protected PriorityEnumCheckerUC() { }
	}
}
