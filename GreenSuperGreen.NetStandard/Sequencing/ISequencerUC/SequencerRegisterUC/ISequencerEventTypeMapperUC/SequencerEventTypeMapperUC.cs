using System;
using System.Collections.Concurrent;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal class SequencerEventTypeMapperUC :	ISequencerEventTypeMapperUC
	{
		private
		ConcurrentDictionary<Type, ISequencerEventMapperUC>
		Mapper { get; } = new ConcurrentDictionary<Type, ISequencerEventMapperUC>()
		;

		public void Add<TEnum>(ISequencerPointUC<TEnum> sequencerPoint) where TEnum : struct
		{
			if (sequencerPoint == null) throw new ArgumentNullException(nameof(sequencerPoint));

			SequencerEventMapperUC<TEnum>.TestEnum();

			ISequencerEventMapperUC enumValueMapper =
			Mapper
			.AddOrUpdate(	SequencerEventMapperUC<TEnum>.GenericInterfaceType,
							//Add - multiple can be created, one is going to win
							type => new SequencerEventMapperUC<TEnum>(),
							//Update, keep existing unless it is null,
							//multiple can be created, one is going to win
							(type, previous) => previous ?? new SequencerEventMapperUC<TEnum>())
			;

			ISequencerEventMapperUC<TEnum>
			genericEnumValueMapper = (ISequencerEventMapperUC<TEnum>)enumValueMapper
			;

			genericEnumValueMapper.Add(sequencerPoint.Registration, sequencerPoint);
		}

		public ISequencerPointUC<TEnum> TryGet<TEnum>(TEnum registration) where TEnum : struct
		{
			SequencerEventMapperUC<TEnum>.TestEnum();

			bool found =
			Mapper.TryGetValue(SequencerEventMapperUC<TEnum>.GenericInterfaceType, out var enumValueMapper)
			&& enumValueMapper != null
			;

			if (!found) return null;

			ISequencerEventMapperUC<TEnum> genericEnumValueMapper = (ISequencerEventMapperUC<TEnum>)enumValueMapper;
			return genericEnumValueMapper.TryGet(registration);
		}
	}
}