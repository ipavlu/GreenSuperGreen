using System;
using System.ComponentModel;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Queues.Test
{
	[TestFixture]
	public class PriorityEnumCheckerUCTest
	{
		public enum CorrectEnum { AA, BB }

		[Flags]
		public enum IncorrectEnum
		{
			AA = 1,
			BB = 2,
			CC = AA | BB
		}

		[Test]
		public void CorrectPriorityEnum()
		{
			PriorityEnumCheckerUC<CorrectEnum>.TestEnum(nameof(CorrectEnum));
		}

		[Test]
		public void IncorrectPriorityEnum()
		{
			Assert.Throws<InvalidEnumArgumentException>(() =>
			{
				PriorityEnumCheckerUC<IncorrectEnum>.TestEnum(nameof(IncorrectEnum));
			})
			;
		}
	}
}
