using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable StaticMemberInGenericType
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace GreenSuperGreen.Reporting
{
	public enum ReportTypeUC
	{
		CVS,
	}

	public enum ReportActionUC
	{
		Update,
		Append,
	}

	public interface IReportUC<in TReportItemEnum> where TReportItemEnum : struct
	{
		ReportTypeUC ReportType { get; }
		IReportUC<TReportItemEnum> NamesAsValues(bool set = true);
		IReportUC<TReportItemEnum> NewLine(bool set = true);
		IReportUC<TReportItemEnum> Report(string report, TReportItemEnum reportItem, ReportActionUC action = ReportActionUC.Update, string appendDelimiter = null);
		string BuildReport();
	}

	public static partial class ReportUC
	{
		public static IReportUC<TReportItemEnum> New<TReportItemEnum>(ReportTypeUC type)
		where TReportItemEnum : struct
		{
			if (type == ReportTypeUC.CVS) return new ReportCVS<TReportItemEnum>();
			return new ReportCVS<TReportItemEnum>();
		}
	}

	public static partial class ReportUC
	{
		private class ReportCVS<TReportItemEnum> : IReportUC<TReportItemEnum> where TReportItemEnum : struct
		{
			private static Type EnumType { get; } = typeof(TReportItemEnum);
			private static Type UnderlyingType { get; } = EnumType.IsEnum ? Enum.GetUnderlyingType(EnumType) : EnumType;
			private static bool IsInt32 { get; } = UnderlyingType == typeof(int);
			private static bool IsEnum { get; } = EnumType.IsEnum;
			private static bool IsNonFlaggedEnum { get; } = !Attribute.IsDefined(EnumType, typeof(FlagsAttribute));
			private static bool EnumIsOK { get; } = IsInt32 && IsEnum && IsNonFlaggedEnum;

			public ReportTypeUC ReportType { get; } = ReportTypeUC.CVS;
			private bool LineDelimiter { get; set; }
			private bool UseNamesAsValues { get; set; }


			private Dictionary<TReportItemEnum,string> Items { get; } =
			Enum
			.GetValues(typeof(TReportItemEnum))
			.Cast<object>()
			.OrderBy(en => (int)en)
			.Select(en => (TReportItemEnum)en)
			.ToDictionary(en => en, en => (string)null)
			;

			public ReportCVS() { if (!EnumIsOK) throw new InvalidOperationException("Incorrect Enum, must be non flagged enum with underlying type int");}

			public IReportUC<TReportItemEnum> NewLine(bool set = true) { LineDelimiter = set; return this; }
			public IReportUC<TReportItemEnum> NamesAsValues(bool set = true) { UseNamesAsValues = set; return this; }

			public IReportUC<TReportItemEnum> Report(string report, TReportItemEnum reportItem, ReportActionUC action = ReportActionUC.Update, string appendDelimiter = null)
			{
				Items.TryGetValue(reportItem, out var current);
				appendDelimiter = current == null ? null : appendDelimiter;
				current = action == ReportActionUC.Update ? report : current;
				current = action == ReportActionUC.Append ? $"{current}{appendDelimiter}{report}" : current;
				Items[reportItem] = current;
				return this;
			}

			public string BuildReport() =>
			Enum
			.GetValues(typeof(TReportItemEnum))
			.Cast<object>()
			.OrderBy(en => (int) en)
			.Select(en => UseNamesAsValues ? en.ToString() : Items[(TReportItemEnum)en] ?? string.Empty)
			.Aggregate(string.Empty, (c, n) => string.IsNullOrEmpty(c)? n : $"{c};{n}", str => LineDelimiter? $"{str}{Environment.NewLine}" : str)
			;

			public override string ToString() => BuildReport();
		}
	}
}
