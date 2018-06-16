using System;
using System.Collections.Concurrent;
using System.ComponentModel;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal class SequencerEventMapperUC<TEnum>
		:	ISequencerEventMapperUC<TEnum>,
			ISequencerEventMapperUC
		where TEnum	:	struct
	{
		/// <summary> <see cref="Type"/> of <see cref="ISequencerEventMapperUC{TEnum}"/> </summary>
		public static Type GenericInterfaceType { get; } = typeof(ISequencerPointUC<TEnum>);
		public static bool IsNotEnum { get; } = !typeof(TEnum).IsEnum;
		public static bool IsFlaggedEnum { get; } = Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute));

		public static void TestEnum()
		{
			if (IsNotEnum) throw new InvalidEnumArgumentException($"{typeof(TEnum).Name} not an Enum!");
			if (IsFlaggedEnum) throw new InvalidEnumArgumentException($"{typeof(TEnum).Name} is flagged Enum! Flagged enums are not supported for {nameof(ISequencerPointUC<TEnum>)}, as multiple combinations are not supported.");
		}

		private
		ConcurrentDictionary<TEnum, ISequencerPointUC<TEnum>>
		Mapper { get; } = new ConcurrentDictionary<TEnum, ISequencerPointUC<TEnum>>()
		;

		public SequencerEventMapperUC()
		{
			TestEnum();
		}

		public void Add(TEnum enumValue, ISequencerPointUC<TEnum> sequencerPoint)
		{
			if (Mapper.TryAdd(enumValue, sequencerPoint)) return;
			throw new ArgumentException($"{nameof(TEnum)}.{enumValue} has been already registered!");
		}

		public ISequencerPointUC<TEnum> TryGet(TEnum enumValue)
		{
			ISequencerPointUC<TEnum> result;
			Mapper.TryGetValue(enumValue, out result);
			return result;
		}
	}
}