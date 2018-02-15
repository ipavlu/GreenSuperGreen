// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System.Threading.Tasks;

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public
		static
		async
		Task<IProductionPointUC>
		TestPointCompleteAsync<TEnum>(this	ISequencerUC sequencer,
											TEnum registration,
											object testArg = null,
											SeqContinuationUC context = SeqContinuationUC.OnCapturedContext)
		where TEnum : struct
		{
			IProductionPointUC prodPoint = await sequencer.TestPointAsync(registration);
			prodPoint?.Complete(testArg, context);
			return prodPoint;
		}
	}
}