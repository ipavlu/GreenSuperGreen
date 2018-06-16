﻿using System;
using System.Threading.Tasks;
using GreenSuperGreen.UnifiedConcurrency;
using NUnit.Framework;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public sealed class MonitorTryEnter : ATestingJob, ITestingJob
	{
		public MonitorTryEnter(int count) : base(count) { }

		#pragma warning disable 618
			private ILockUC Lock { get; } = new MonitorLockUC();
		#pragma warning restore 618

		protected override bool ExclusiveAccess()
		{
			using (EntryBlockUC entry = Lock.TryEnter())
			{
				if (!entry.HasEntry)
				{
					return true;//no entry, keep trying
				}
				return ProcessExclusively();
			}
		}
	}

	[TestFixture]
	public partial class UnifiedConcurrency
	{
		[Test]
		public async Task MonitorTryEnterTest()
		{
			using (ITestingJob job = new MonitorTryEnter(1000000))
			{
				await job.Execute(Environment.ProcessorCount);
			}
		}
	}
}
