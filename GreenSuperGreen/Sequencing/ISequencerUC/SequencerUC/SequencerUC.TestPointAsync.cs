// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public
		static
		ITestPointUC
		TestPointAsync<TEnum>(this ISequencerUC sequencer, TEnum registration)
		where TEnum : struct
		{
			if (!(sequencer is SequencerRegisterUC register)) return TestPointUC.EmptyCompleted;

			ISequencerExceptionRegister exceptionRegister = register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			ISequencerPointUC<TEnum> seqPoint = register.TryGet(registration);

			return
			seqPoint?.TestPoint(taskRegister, exceptionRegister) ??
			TestPointUC.EmptyCompleted
			;
		}
	}
}
