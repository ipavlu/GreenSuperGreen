using System;

// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Timing
{
	public interface IRealTimeSource
	{
		DateTime GetUtcNow();
	}

	public class RealTimeSource : IRealTimeSource
	{
		public DateTime GetUtcNow() => DateTime.UtcNow;
	}
}
