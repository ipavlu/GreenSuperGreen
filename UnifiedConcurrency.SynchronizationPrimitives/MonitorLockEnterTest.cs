using System;
using System.CodeDom;
using System.Threading;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	/// <summary>
	/// <para/> <see cref="MonitorLockUC"/> is based on .Net <see cref="Monitor"/>, the synchronization primitive behind c# lock keyword.
	/// <para/> Wrapper does not support recursive call and does not protect against recursive call even when the <see cref="Monitor"/> supports it!
	/// <para/> Enter and Exit MUST BE DONE on same thread! The lock is thread affine!
	/// <para/> awaiting inside entry block NOT SUPPORTED, causing synchronization error!
	/// </summary>
	[Obsolete("Potentially harmfull! You would be served better with LockUC! Awaiting inside entry block or attempt to exit entry block in different thread is not supported!")]
	internal class MonitorLockUC : ISimpleLockUC
	{
		private object ObjLock { get; } = new object();
		private EntryCompletionUC EntryCompletion { get; }

		public SyncPrimitiveCapabilityUC Capability { get; } = 0
		| SyncPrimitiveCapabilityUC.Enter
		| SyncPrimitiveCapabilityUC.TryEnter
		| SyncPrimitiveCapabilityUC.TryEnterWithTimeout
		| SyncPrimitiveCapabilityUC.NonCancellable
		| SyncPrimitiveCapabilityUC.NonRecursive
		| SyncPrimitiveCapabilityUC.ThreadAffine
		;

		public MonitorLockUC()
		{
			EntryCompletion = new EntryCompletionUC(Exit);
		}

		private void Exit() => Monitor.Exit(ObjLock);

		public EntryBlockUC Enter()
		{
			Monitor.Enter(ObjLock);
			return new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion);
		}

		public EntryBlockUC TryEnter()
		{
			return Monitor.TryEnter(ObjLock)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}

		public EntryBlockUC TryEnter(int milliseconds)
		{
			return Monitor.TryEnter(ObjLock, milliseconds)
			? new EntryBlockUC(EntryTypeUC.Exclusive, EntryCompletion)
			: EntryBlockUC.RefusedEntry
			;
		}
	}


	public sealed class MonitorLockEnter : ATestingJob, ITestingJob
	{
		public MonitorLockEnter(int count) : base(count) { }

		private ISimpleLockUC Lock { get; } = new MonitorLockUC();

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.Enter())
			{
				if (!entry.HasEntry) throw new Exception("hmmmmmmmmm");
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		[Test]
		public async Task MonitorLockEnterTest()
		{
			using (ITestingJob job = new LockEnter(1000000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
