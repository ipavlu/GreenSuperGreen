// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal interface ISequencerEventMapperUC
	{
	}

	internal interface ISequencerEventMapperUC<TEnum>
		where TEnum : struct
	{
		void Add(TEnum enumValue, ISequencerPointUC<TEnum> sequencerPoint);
		ISequencerPointUC<TEnum> TryGet(TEnum enumValue);
	}
}