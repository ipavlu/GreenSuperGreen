using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.UnifiedConcurrency
{
	[Flags]
	public enum SyncPrimitiveCapabilityUC
	{
		/// <summary> Supports <see cref="ILockUC.Enter"/> </summary>
		Enter = (1 << 0),
		/// <summary> Supports <see cref="ILockUC.TryEnter()"/> </summary>
		TryEnter = (1 << 1),
		/// <summary> Supports <see cref="ILockUC.TryEnter(int)"/> </summary>
		TryEnterWithTimeout = (1 << 2),
		/// <summary> Recursive calls supported. Recursiveness should be avoided at all costs! </summary>
		Recursive = (1 << 3),
		/// <summary> Recursive calls not supported. </summary>
		NonRecursive = (1 << 4),
		/// <summary>
		/// <para/> Thread affinity is required by legacy synchronization primitives like .net <see cref="System.Threading.Monitor"/>
		/// <para/> Enter and Exit has to be executed on same thread!
		/// <para/> awaiting between Entry and Exit points will crash synchronization!
		/// </summary>
		ThreadAffine = (1 << 5),
		/// <summary> Enter and Exit is not thread affine, can occur on different threads. </summary>
		NonThreadAffine = (1 << 6),
		/// <summary> Entry is cancellable based, improving application shutdown responsiveness! </summary>
		Cancellable = (1 << 7),
		/// <summary> Non Cancellable. </summary>
		NonCancellable = (1 << 8),

	}

	public static class SyncPrimitiveCapabilityUCExtensions
	{
		public static bool Is(this SyncPrimitiveCapabilityUC src, SyncPrimitiveCapabilityUC flags)
		=> flags == 0 ? src == 0  : (src & flags) == flags;

		public static bool IsNot(this SyncPrimitiveCapabilityUC src, SyncPrimitiveCapabilityUC flags)
		=> !Is(src, flags);
	}
}
