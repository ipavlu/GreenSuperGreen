// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	/// <summary>
	/// <para/> <see cref="ISequencerUC"/> interface is used only to store instances
	/// <para/> and for extension methods, which will know how to invoke sequencer
	/// <para/> functions returning awaiters and ensure NULL safety in production code,
	/// <para/> where every call to sequencer extension method will return completed awaiter,
	/// <para/> thus ensuring immediate execution of continuation after
	/// <para/> await <see cref="ISequencerUC"/>.NullSafeAsyncCall(...) or
	/// <para/> <see cref="ISequencerUC"/>.NullSafeCall(...).
	/// </summary>
	public interface ISequencerUC
	{
		//DO NOT EXTEND THIS INTERFACE, INSTANCE STORAGE ONLY
	}
}