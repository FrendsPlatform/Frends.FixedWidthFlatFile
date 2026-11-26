using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Helpers;

internal static class FlatFileParser
{
    internal static ParsedFlatFile Parse(string content, Options options, CancellationToken cancellationToken)
    {
        var rows = ReadLines(content);

        cancellationToken.ThrowIfCancellationRequested();

        var headers = new List<string>();
        switch (options.HeaderRow)
        {
            case HeaderRowType.Delimited:
                headers = SplitToList(rows[0], Convert.ToChar(options.HeaderDelimiter));
                rows.RemoveAt(0);
                break;
            case HeaderRowType.FixedWidth:
                headers = SplitToList(rows[0], options.ColumnSpecifications);
                rows.RemoveAt(0);
                break;
        }

        if (options.SkipRows)
        {
            rows = rows.Skip(Math.Max(options.SkipRowsFromTop, 0)).ToList();
            rows = rows.Take(Math.Max(rows.Count - Math.Max(options.SkipRowsFromBottom, 0), 0)).ToList();
        }

        var culture = string.IsNullOrWhiteSpace(options.Culture)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(options.Culture.Trim(), predefinedOnly: true);

        var parsedRows = new List<object[]>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            parsedRows.Add(ParseDataRow(row, options.ColumnSpecifications, culture));
        }

        return new ParsedFlatFile(ResolveColumnNames(options.ColumnSpecifications, headers), parsedRows);
    }

    private static List<string> ReadLines(string content)
    {
        var allLines = new List<string>();
        using var reader = new StringReader(content);

        while (reader.ReadLine() is { } line)
        {
            // skip empty lines
            if (!string.IsNullOrWhiteSpace(line))
                allLines.Add(line);
        }

        return allLines;
    }

    private static List<string> SplitToList(string row, char delimiter)
    {
        return row
            .Split(new[] { delimiter }, StringSplitOptions.None)
            .ToList();
    }

    private static List<string> SplitToList(string row, ColumnSpecification[] columnSpecifications)
    {
        try
        {
            var values = new List<string>();

            var startIndex = 0;
            foreach (var columnSpec in columnSpecifications)
            {
                var value = row.Substring(startIndex, columnSpec.Length);
                values.Add(value.Trim());

                // move substring start index
                startIndex += columnSpec.Length;
            }

            return values;
        }
        catch (Exception ex)
        {
            // throw custom exception for more descriptive information
            throw new InvalidDataException("Data row did not match column specifications.", ex);
        }
    }

    /// <summary>
    /// Resolves the name of each column. A name set in the column specification wins over the header value,
    /// columns without any name get Field_{column number}. Duplicate names are made unique with a _{index} suffix.
    /// </summary>
    private static string[] ResolveColumnNames(
        ColumnSpecification[] columnSpecifications,
        IReadOnlyList<string> headers)
    {
        var usedNames = new HashSet<string>();
        var names = new string[columnSpecifications.Length];

        for (var i = 0; i < columnSpecifications.Length; i++)
        {
            var name = columnSpecifications[i].Name;

            if (string.IsNullOrEmpty(name) && i < headers.Count)
                name = headers[i];

            if (string.IsNullOrEmpty(name))
                name = $"Field_{i + 1}";

            var uniqueName = name;
            var renameIndex = 1;
            while (!usedNames.Add(uniqueName))
            {
                uniqueName = $"{name}_{renameIndex}";
                renameIndex++;
            }

            try
            {
                System.Xml.XmlConvert.VerifyNCName(uniqueName);
            }
            catch (System.Xml.XmlException ex)
            {
                throw new InvalidDataException(
                    $"Column name '{uniqueName}' (column {i + 1}) is not a valid XML element name. Set a valid name in the column specification.",
                    ex);
            }

            names[i] = uniqueName;
        }

        return names;
    }

    private static object[] ParseDataRow(string row, ColumnSpecification[] columnSpecifications, CultureInfo culture)
    {
        var rowValues = SplitToList(row, columnSpecifications);
        var parsedValues = new object[rowValues.Count];

        for (var i = 0; i < rowValues.Count; i++)
        {
            parsedValues[i] = string.IsNullOrWhiteSpace(rowValues[i])
                ? null
                : ParseValue(rowValues[i], columnSpecifications[i], culture);
        }

        return parsedValues;
    }

    private static object ParseValue(string value, ColumnSpecification columnSpecification, CultureInfo culture)
    {
        switch (columnSpecification.Type)
        {
            case ColumnType.Boolean:
                return bool.Parse(value);
            case ColumnType.Char:
                return char.Parse(value);
            case ColumnType.DateTime:
                return string.IsNullOrEmpty(columnSpecification.DateTimeFormat)
                    ? DateTime.Parse(value, culture)
                    : DateTime.ParseExact(value, columnSpecification.DateTimeFormat, culture);
            case ColumnType.Decimal:
                return decimal.Parse(value, culture);
            case ColumnType.Double:
                return double.Parse(value, culture);
            case ColumnType.Int:
                return int.Parse(value, culture);
            case ColumnType.Long:
                return long.Parse(value, culture);
            default:
                return value;
        }
    }
}
