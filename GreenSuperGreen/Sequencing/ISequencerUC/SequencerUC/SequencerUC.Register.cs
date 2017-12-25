// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		/// <summary>
		/// Registration to <see cref="ISequencerUC"/>
		/// should be done in UnitTest project only
		/// </summary>
		public
		static
		ISequencerUC
		Register<TEnum>(this	ISequencerUC sequencer,
								ISequencerPointUC<TEnum> sequencerPoint)
		where TEnum : struct
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			register.Add(sequencerPoint);

			return sequencer;
		}

		/// <summary>
		/// Registration to <see cref="ISequencerUC"/>
		/// should be done in UnitTest project only
		/// </summary>
		public
		static
		ISequencerUC
		Register<TEnum>(this	ISequencerUC sequencer,
										TEnum registration,
										ISequencerStrategyUC sequencerStrategy)
		where TEnum : struct
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();

			ISequencerPointUC <TEnum> sequencerPoint = new SequencerPointUC<TEnum>(registration, sequencerStrategy);
			register.Add(sequencerPoint);

			return sequencer;
		}
	}
}