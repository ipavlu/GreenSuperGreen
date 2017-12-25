using System;
using System.Threading;
using GreenSuperGreen.Async;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable RedundantExtendsListEntry

namespace GreenSuperGreen.Sequencing
{
	public class TestPointUC
	:	ACompletionUC<TestPointUC, /*await result*/ IProductionPointUC>,
		ITestPointUC,
		ICompletionUC< /*await result*/ IProductionPointUC>
	{
		private int _completed;
		private ISequencerTaskRegister TaskRegister { get; }
		private ISequencerExceptionRegister ExceptionRegister { get; }

		private TestPointUC() { }
		public TestPointUC(ISequencerTaskRegister taskRegister, ISequencerExceptionRegister exceptionRegister)
		{
			TaskRegister = taskRegister;
			ExceptionRegister = exceptionRegister;
			exceptionRegister.Token.Register(OnCancel);
		}

		private void OnCancel()
		{
			SetException(ExceptionRegister.TryGetException());
		}
		public void Complete(IProductionPointUC productionPoint = null)
		{
			if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)//allow completion only once
			{
				SetCompletion(productionPoint);
				return;
			}
			Exception ex = new InvalidOperationException($"{nameof(TestPointUC)}: Calling {nameof(Complete)} or {nameof(Fail)} multiple times!");
			ExceptionRegister?.RegisterException(ex);
			throw ex;
		}

		public void Fail(Exception exception = null)
		{
			if (Interlocked.CompareExchange(ref _completed, 1, 0) == 0)//allow completion only once
			{
				SetException(exception ?? new InvalidOperationException($"{nameof(TestPointUC)}.{nameof(Fail)}: Reason for Exception was not provided."));
				return;
			}
			Exception ex = new InvalidOperationException($"{nameof(TestPointUC)}: Calling {nameof(Complete)} or {nameof(Fail)} multiple times!");
			ExceptionRegister?.RegisterException(ex);
			throw ex;
		}

		public static ITestPointUC EmptyCompleted { get; } = CreateEmptyCompleted();

		private static ITestPointUC CreateEmptyCompleted()
		{
			TestPointUC point = new TestPointUC();
			point.Complete();
			return point;
		}
	}
}
