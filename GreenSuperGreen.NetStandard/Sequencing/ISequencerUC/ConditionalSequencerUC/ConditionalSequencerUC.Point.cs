using System;
using System.Diagnostics;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class ConditionalSequencerUC
	{
		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public
		static
		void Point<TEnum>(	ISequencerUC sequencer,
							Func<bool> condition,
							SeqPointTypeUC seqPointTypeUC,
							TEnum registration,
							object arg = null,
							Action<object> injectContinuation = null)
		where TEnum : struct
		{
			sequencer?.Point(seqPointTypeUC, registration, condition, arg, injectContinuation);
		}

		/// <summary> Avoiding boxing of <see cref="ValueType"/> argument in case Sequencer is null </summary>
		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public
		static
		void PointArg<TEnum,TArg>(	ISequencerUC sequencer,
									Func<bool> condition,
									SeqPointTypeUC seqPointTypeUC,
									TEnum registration,
									TArg arg,
									Action<object> injectContinuation = null)
		where TEnum : struct
		{
			sequencer?.Point(seqPointTypeUC, registration, condition, arg, injectContinuation);
		}

		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public
		static
		void Point<TEnum>(ISequencerUC sequencer,
							SeqPointTypeUC seqPointTypeUC,
							TEnum registration,
							object arg = null,
							Action<object> injectContinuation = null)
		where TEnum : struct
		{
			sequencer?.Point(seqPointTypeUC, registration, arg, injectContinuation);
		}

		/// <summary> Avoiding boxing of <see cref="ValueType"/> argument in case Sequencer is null </summary>
		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		public
		static
		void PointArg<TEnum,TArg>(	ISequencerUC sequencer,
									SeqPointTypeUC seqPointTypeUC,
									TEnum registration,
									TArg arg,
									Action<object> injectContinuation = null)
		where TEnum : struct
		{
			sequencer?.Point(seqPointTypeUC, registration, arg, injectContinuation);
		}
	}
}