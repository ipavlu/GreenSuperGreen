using System;
using System.Diagnostics;

// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Sequencing
{
	[Flags]
	public enum ConditionalSettingsUC
	{
		/// <summary> <see cref="ConditionalSequencerUC"/> wont be activated </summary>
		NONE = 0,
		/// <summary> <see cref="ConditionalSequencerUC"/> could be activated </summary>
		DEBUG = 1 << 0,
		/// <summary> <see cref="ConditionalSequencerUC"/> could be activated, this one is for unit testing in release build </summary>
		INJECTUNITTESTCODE = 1 << 1
	}

	/// <summary>
	/// This class is looking for available conditionnals
	/// </summary>
	public static class ConditionalCheckUC
	{
		public static ConditionalSettingsUC Settings { get; } = Check();

		private static ConditionalSettingsUC Check()
		{
			ConditionalSettingsUC settings = ConditionalSettingsUC.NONE;
			CheckDEBUG(flag => settings |= flag);
			CheckINJECTUNITTESTCODE(flag => settings |= flag);
			return settings;
		}

		[Conditional(nameof(ConditionalSettingsUC.DEBUG))]
		private static void CheckDEBUG(Action<ConditionalSettingsUC> setFlag)
		{
			setFlag.Invoke(ConditionalSettingsUC.DEBUG);
		}

		[Conditional(nameof(ConditionalSettingsUC.INJECTUNITTESTCODE))]
		private static void CheckINJECTUNITTESTCODE(Action<ConditionalSettingsUC> setFlag)
		{
			setFlag.Invoke(ConditionalSettingsUC.INJECTUNITTESTCODE);
		}

		public static bool IsDefined(ConditionalSettingsUC settings) =>
		settings == ConditionalSettingsUC.NONE
		? Settings == ConditionalSettingsUC.NONE
		: (Settings & settings) == settings
		;

		public static bool IsNotDefined(ConditionalSettingsUC settings) => !IsDefined(settings);
	}
}
