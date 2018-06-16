using System;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	public interface ISequencerTaskRegister
	{
		ISequencerUC SequencerUC { get; }

		void RegisterTask(Task task);

		void Run(Action action);
		void Run(object obj, Action<object> action);
		void Run<TParameter>(TParameter parameter, Action<ISequencerUC,TParameter> action);
		void Run<TResult>(Func<TResult> func);
		void RunAsync(Func<Task> func);
		void RunAsync<TResult>(Func<Task<TResult>> func);
		void Run(Action<ISequencerUC> action);
		void Run(object obj, Action<ISequencerUC,object> action);
		void Run<TResult>(Func<ISequencerUC,TResult> func);
		void RunAsync(Func<ISequencerUC,Task> func);
		void RunAsync<TResult>(Func<ISequencerUC,Task<TResult>> func);

		Task WhenAll();
	}
}