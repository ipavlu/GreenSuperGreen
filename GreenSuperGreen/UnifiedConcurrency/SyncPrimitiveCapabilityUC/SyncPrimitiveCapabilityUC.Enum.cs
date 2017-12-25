﻿using System;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global

namespace GreenSuperGreen.UnifiedConcurrency
{
	[Flags]
	public enum SyncPrimitiveCapabilityUC
	{
		/// <summary> Supports <see cref="ISimpleLockUC.Enter"/> </summary>
		Enter	= (1 << 0),
		/// <summary> Supports <see cref="ISimpleLockUC.TryEnter()"/> </summary>
		TryEnter = (1 << 1),
		/// <summary> Supports <see cref="ISimpleLockUC.TryEnter(int)"/> </summary>
		TryEnterWithTimeout = (1 << 2),
		/// <summary>
		/// <para/> Reentrancy should be avoided at all costs!
		/// <para/> Supported only on legacy synchronization primitives based .net <see cref="System.Threading.Monitor"/>
		/// </summary>
		Reentrant = (1 << 3),
		/// <summary> Reentrancy not supported </summary>
		NonReentrant = (1 << 4),
		/// <summary>
		/// <para/> Thread afinity is required by legacy synchronization primitives like .net <see cref="System.Threading.Monitor"/>
		/// <para/> Enter and Exit has to be executed on same thread!
		/// <para/> awaiting between Entry and Exit points will crash synchronization!
		/// </summary>
		ThreadAffine = (1 << 5),
		/// <summary> Enter and Exit is not thread affine, can occur on different threads. </summary>
		NonThreadAffine = (1 << 6),
		/// <summary> Entry is cancellable based, improving application shutdown responsivness! </summary>
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
