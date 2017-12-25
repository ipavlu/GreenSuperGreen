// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public static ISequencerUC TryReThrowException(this ISequencerUC sequencer)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();

			return sequencer;
		}
	}
}
