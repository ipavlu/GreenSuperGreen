using System;
using System.Threading.Tasks;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public static partial class SequencerUC
	{
		public static ISequencerUC Run(this ISequencerUC sequencer, Action action)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(action);
			return sequencer;
		}

		public static ISequencerUC Run(this ISequencerUC sequencer, Action<ISequencerUC> action)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(action);
			return sequencer;
		}

		public static ISequencerUC Run(this ISequencerUC sequencer, object obj, Action<object> action)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;
			
			taskRegister.Run(obj,action);
			return sequencer;
		}

		public
		static
		ISequencerUC Run<TParameter>(this	ISequencerUC sequencer,
											TParameter parameter,
											Action<ISequencerUC, TParameter> action)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(parameter, action);
			return sequencer;
		}

		public static ISequencerUC Run(this ISequencerUC sequencer, object obj, Action<ISequencerUC, object> action)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(obj, action);
			return sequencer;
		}

		public static ISequencerUC Run<TResult>(this ISequencerUC sequencer, Func<TResult> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(func);
			return sequencer;
		}

		public static ISequencerUC Run<TResult>(this ISequencerUC sequencer, Func<ISequencerUC,TResult> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.Run(func);
			return sequencer;
		}

		public static ISequencerUC Run(this ISequencerUC sequencer, Func<Task> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.RunAsync(func);
			return sequencer;
		}

		public static ISequencerUC Run(this ISequencerUC sequencer, Func<ISequencerUC,Task> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.RunAsync(func);
			return sequencer;
		}

		public static ISequencerUC Run<TResult>(this ISequencerUC sequencer, Func<Task<TResult>> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.RunAsync(func);
			return sequencer;
		}

		public static ISequencerUC Run<TResult>(this ISequencerUC sequencer, Func<ISequencerUC, Task<TResult>> func)
		{
			SequencerRegisterUC register = sequencer as SequencerRegisterUC;
			if (register == null) return sequencer;

			register.ExceptionRegister.TryReThrowException();
			ISequencerTaskRegister taskRegister = register.TaskRegister;

			taskRegister.RunAsync(func);
			return sequencer;
		}
	}
}
