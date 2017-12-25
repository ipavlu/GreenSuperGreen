using System;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public
		static
		void Point<TEnum>(this ISequencerUC sequencer,
								SeqPointTypeUC seqPointTypeUC,
								TEnum registration,
								Func<bool> condition,
								object arg = null,
								Action<object> injectContinuation = null)
		where TEnum : struct
		{
			var rslt = sequencer.PointAsync(seqPointTypeUC, registration, condition, arg, injectContinuation);
			rslt.GetResult();//sync waiting unless completed awaiter was provided
		}

		public
		static
		void Point<TEnum>(this	ISequencerUC sequencer,
								SeqPointTypeUC seqPointTypeUC,
								TEnum registration,
								object arg = null,
								Action<object> injectContinuation = null)
		where TEnum : struct
		{
			var rslt = sequencer.PointAsync(seqPointTypeUC, registration, arg, injectContinuation);
			rslt.GetResult();//sync waiting unless completed awaiter was provided
		}
	}
}