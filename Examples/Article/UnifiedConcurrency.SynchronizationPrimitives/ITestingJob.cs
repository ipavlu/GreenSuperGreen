using System;
using System.Threading.Tasks;

// ReSharper disable RedundantExtendsListEntry

namespace UnifiedConcurrency.SynchronizationPrimitives
{
	public interface ITestingJob : IDisposable
	{
		Task Execute(int tasks);
	}
}
