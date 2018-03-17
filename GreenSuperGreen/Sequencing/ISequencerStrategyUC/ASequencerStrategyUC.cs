using System;
using System.Collections.Generic;
using GreenSuperGreen.Async;
using GreenSuperGreen.UnifiedConcurrency;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable MemberCanBePrivate.Global

namespace GreenSuperGreen.Sequencing
{
	public abstract class ASequencerStrategyUC : ISequencerStrategyUC
	{
		protected virtual ILockUC Lock { get; } = new SpinLockUC();

		protected virtual Queue<IProductionPointUC> ProductionPointQueue { get; } = new Queue<IProductionPointUC>();
		protected virtual Queue<ITestPointUC> TestPointQueue { get; } = new Queue<ITestPointUC>();

		public
		virtual
		ICompletionUC
		ProductionPoint(ISequencerTaskRegister taskRegister,
						ISequencerExceptionRegister exceptionRegister,
						SeqPointTypeUC seqPointTypeUC,
						object arg = null,
						Action<object> injectContinuation = null)
		{
			ProductionPointUC productionPoint = new ProductionPointUC(taskRegister, exceptionRegister, seqPointTypeUC, arg, injectContinuation);
			IProductionPointUC productionMatch = null;
			ITestPointUC testMatch = null;
			using (Lock.Enter())
			{
				ProductionPointQueue.Enqueue(productionPoint);
				if (ProductionPointQueue.Count > 0 && TestPointQueue.Count > 0)
				{
					productionMatch = ProductionPointQueue.Dequeue();
					testMatch = TestPointQueue.Dequeue();
				}
			}
			testMatch?.Complete(productionMatch);
			return productionPoint;
		}

		public
		virtual
		ITestPointUC
		TestPoint(	ISequencerTaskRegister taskRegister,
					ISequencerExceptionRegister exceptionRegister)
		{
			TestPointUC testPoint = new TestPointUC(taskRegister, exceptionRegister);
			IProductionPointUC productionMatch = null;
			ITestPointUC testMatch = null;
			using (Lock.Enter())
			{
				TestPointQueue.Enqueue(testPoint);
				if (ProductionPointQueue.Count > 0 && TestPointQueue.Count > 0)
				{
					productionMatch = ProductionPointQueue.Dequeue();
					testMatch = TestPointQueue.Dequeue();
				}
			}
			testMatch?.Complete(productionMatch);
			return testPoint;
		}
	}
}