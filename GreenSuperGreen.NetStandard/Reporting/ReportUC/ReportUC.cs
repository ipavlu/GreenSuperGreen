using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
		IReportUC<TReportItemEnum> Report(	string report,
											TReportItemEnum reportItem,
											ReportActionUC action = ReportActionUC.Update,
											string appendDelimiter = null);
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

			private static readonly ImmutableArray<TReportItemEnum> OrderedEnumValues = ImmutableArray<TReportItemEnum>.Empty;

			static ReportCVS()
			{
				if (!EnumIsOK) return;

				Enum
				.GetValues(typeof(TReportItemEnum))
				.Cast<TReportItemEnum>()
				.Select(key => new KeyValuePair<TReportItemEnum,int>(key, (int)(object)key))
				.OrderBy(kvp => kvp.Value)
				.Select(x => x.Key)
				.ToImmutableArray()
				.AssignOut(out OrderedEnumValues)
				;
			}

			public ReportTypeUC ReportType { get; } = ReportTypeUC.CVS;
			private bool LineDelimiter { get; set; }
			private bool UseNamesAsValues { get; set; }


			private Dictionary<TReportItemEnum, string> Items { get; } = OrderedEnumValues.ToDictionary(x => x, x => (string)null);

			public ReportCVS()
			{
				if (EnumIsOK) return;
				string message = $"Something is wrong with enum type {nameof(TReportItemEnum)}:{EnumType.Name}!";
				message = !IsEnum ? $"{message}{Environment.NewLine}Type: {EnumType.Name} is not an enum type!" : message;
				message = IsEnum && !IsNonFlaggedEnum ? $"{message}{Environment.NewLine}EnumType: {EnumType.Name} can not be flagged enum!" : message;
				message = IsEnum && !IsInt32  ? $"{message}{Environment.NewLine}EnumType: {EnumType.Name} has wrong underlying type {UnderlyingType.Name}: {typeof(int).Name} is required!" : message;
				throw new  InvalidOperationException(message);
			}

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

			public string BuildReport()
			{
				int i = -1;
				return
				OrderedEnumValues
				.Select(en => UseNamesAsValues ? en.ToString() : Items[en] ?? string.Empty)
				.Aggregate(string.Empty, (c, n) => ++i == 0 ? $"{n}" : $"{c};{n}", str => LineDelimiter ? $"{str}{Environment.NewLine}" : str)
				;
			}

			public override string ToString() => BuildReport();
		}
	}
}
