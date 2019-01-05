// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public static ISequencerUC TryReThrowException(this ISequencerUC sequencer)
		{
			if (!(sequencer is SequencerRegisterUC register)) return sequencer;

			register.ExceptionRegister.TryReThrowException();

			return sequencer;
		}
	}
}
