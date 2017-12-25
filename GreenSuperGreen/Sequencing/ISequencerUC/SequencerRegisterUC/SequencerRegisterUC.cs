// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	internal class SequencerRegisterUC : ISequencerUC, ISequencerEventTypeMapperUC
	{
		private SequencerEventTypeMapperUC EventRegister { get; }
		public SequencerExceptionRegister ExceptionRegister { get; }
		public SequencerTaskRegister TaskRegister { get; }
		
		public SequencerRegisterUC()
		{
			EventRegister = new SequencerEventTypeMapperUC();
			ExceptionRegister = new SequencerExceptionRegister();
			TaskRegister = new SequencerTaskRegister(this, ExceptionRegister);
		}

		public void Add<TEnum>(ISequencerPointUC<TEnum> sequencerPoint) where TEnum : struct
		{
			EventRegister.Add(sequencerPoint);
		}

		public ISequencerPointUC<TEnum> TryGet<TEnum>(TEnum registration) where TEnum : struct
		{
			return EventRegister.TryGet(registration);
		}
	}
}
