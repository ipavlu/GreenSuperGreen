using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public static Task WhenAll(this ISequencerUC sequencer)
		{
			return (sequencer as SequencerRegisterUC)?.TaskRegister?.WhenAll() ?? Task.CompletedTask;
		}
	}
}
