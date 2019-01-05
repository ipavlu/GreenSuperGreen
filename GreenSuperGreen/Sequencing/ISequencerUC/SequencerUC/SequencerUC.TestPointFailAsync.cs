// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public
		static
		async
		Task<IProductionPointUC>
		TestPointFailAsync<TEnum>(this	ISequencerUC sequencer,
										TEnum registration,
										Exception ex = null)
		where TEnum : struct
		{
			IProductionPointUC prodPoint = await sequencer.TestPointAsync(registration);
			prodPoint?.Fail(ex);
			return prodPoint;
		}
	}
}