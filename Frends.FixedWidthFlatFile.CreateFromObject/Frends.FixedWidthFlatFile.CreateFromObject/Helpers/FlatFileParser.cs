using System.Globalization;
using Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Helpers;

internal static class FlatFileParser
{
    internal static List<string> GetColumnNames(ColumnSpecification[] columnSpecifications)
    {
        var names = new List<string>(columnSpecifications.Length);
        var usedNames = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < columnSpecifications.Length; index++)
        {
            var name = string.IsNullOrEmpty(columnSpecifications[index].Name)
                ? $"Field_{index + 1}"
                : columnSpecifications[index].Name;
            var uniqueName = name;
            var suffix = 1;
            while (!usedNames.Add(uniqueName))
            {
                uniqueName = $"{name}_{suffix++}";
            }

            names.Add(uniqueName);
        }

        return names;
    }

    internal static string FormatRow(object[] values, ColumnSpecification[] columnSpecifications)
    {
        var formattedValues = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            var specification = columnSpecifications[index];
            var text = FormatValue(value, specification);
            if (text.Length > specification.Length)
            {
                throw new InvalidDataException(
                    $"Value for column '{specification.Name}' exceeds configured width {specification.Length}.");
            }

            formattedValues[index] = text.PadRight(specification.Length);
        }

        return string.Concat(formattedValues);
    }

    internal static string FormatHeader(List<string> columnNames, ColumnSpecification[] columnSpecifications)
    {
        var formattedNames = new string[columnNames.Count];
        for (var index = 0; index < columnNames.Count; index++)
        {
            if (columnNames[index].Length > columnSpecifications[index].Length)
            {
                throw new InvalidDataException(
                    $"Header for column '{columnNames[index]}' exceeds configured width {columnSpecifications[index].Length}.");
            }

            formattedNames[index] = columnNames[index].PadRight(columnSpecifications[index].Length);
        }

        return string.Concat(formattedNames);
    }

    private static string FormatValue(object value, ColumnSpecification specification)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (!Enum.IsDefined(specification.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(specification.Type), "Unsupported column type.");
        }

        return specification.Type switch
        {
            ColumnType.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            ColumnType.Int => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ColumnType.Long => Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ColumnType.Decimal => Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ColumnType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ColumnType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString(),
            ColumnType.Char => Convert.ToChar(value, CultureInfo.InvariantCulture).ToString(),
            ColumnType.DateTime => FormatDateTime(value, specification.DateTimeFormat),
            _ => throw new ArgumentOutOfRangeException(nameof(specification.Type), "Unsupported column type."),
        };
    }

    private static string FormatDateTime(object value, string dateTimeFormat)
    {
        var dateTime = value switch
        {
            DateTime date => date,
            DateTimeOffset date => date.DateTime,
            _ => DateTime.Parse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
        };

        return string.IsNullOrEmpty(dateTimeFormat)
            ? dateTime.ToString(CultureInfo.InvariantCulture)
            : dateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
    }
}
