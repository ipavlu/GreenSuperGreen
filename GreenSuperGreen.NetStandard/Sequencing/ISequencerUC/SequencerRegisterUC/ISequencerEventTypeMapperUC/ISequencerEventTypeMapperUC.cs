// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal interface ISequencerEventTypeMapperUC
	{
		void Add<TEnum>(ISequencerPointUC<TEnum> sequencerPoint) where TEnum : struct;
		ISequencerPointUC<TEnum> TryGet<TEnum>(TEnum registration) where TEnum : struct;
	}
}