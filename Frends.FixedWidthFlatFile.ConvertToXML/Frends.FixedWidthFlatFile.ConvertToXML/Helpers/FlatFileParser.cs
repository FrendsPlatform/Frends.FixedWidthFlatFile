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
    internal static ParsedFlatFile Parse(Input input, Options options, CancellationToken cancellationToken)
    {
        var rows = ReadLines(input.FlatFileContent);

        cancellationToken.ThrowIfCancellationRequested();

        var headers = new List<string>();
        switch (input.HeaderRow)
        {
            case HeaderRowType.Delimited:
                headers = SplitToList(rows[0], Convert.ToChar(input.HeaderDelimiter));
                rows.RemoveAt(0);
                break;
            case HeaderRowType.FixedWidth:
                headers = SplitToList(rows[0], input.ColumnSpecifications);
                rows.RemoveAt(0);
                break;
        }

        if (options.SkipRows)
        {
            rows = rows.Skip(Math.Max(options.SkipRowsFromTop, 0)).ToList();
            rows = rows.Take(Math.Max(rows.Count - Math.Max(options.SkipRowsFromBottom, 0), 0)).ToList();
        }

        var parsedRows = new List<object[]>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            parsedRows.Add(ParseDataRow(row, input.ColumnSpecifications));
        }

        return new ParsedFlatFile(ResolveColumnNames(input.ColumnSpecifications, headers), parsedRows);
    }

    private static List<string> ReadLines(string content)
    {
        var allLines = new List<string>();
        using var reader = new StringReader(content);

        string line;
        while ((line = reader.ReadLine()) != null)
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
    private static string[] ResolveColumnNames(ColumnSpecification[] columnSpecifications, IReadOnlyList<string> headers)
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

            names[i] = uniqueName;
        }

        return names;
    }

    private static object[] ParseDataRow(string row, ColumnSpecification[] columnSpecifications)
    {
        var rowValues = SplitToList(row, columnSpecifications);
        var parsedValues = new object[rowValues.Count];

        for (var i = 0; i < rowValues.Count; i++)
        {
            parsedValues[i] = string.IsNullOrWhiteSpace(rowValues[i])
                ? null
                : ParseValue(rowValues[i], columnSpecifications[i]);
        }

        return parsedValues;
    }

    private static object ParseValue(string value, ColumnSpecification columnSpecification)
    {
        switch (columnSpecification.Type)
        {
            case ColumnType.Boolean:
                return bool.Parse(value);
            case ColumnType.Char:
                return char.Parse(value);
            case ColumnType.DateTime:
                return string.IsNullOrEmpty(columnSpecification.DateTimeFormat)
                    ? DateTime.Parse(value)
                    : DateTime.ParseExact(value, columnSpecification.DateTimeFormat, CultureInfo.InvariantCulture);
            case ColumnType.Decimal:
                return decimal.Parse(value, GetNumberCulture(value));
            case ColumnType.Double:
                return double.Parse(value, GetNumberCulture(value));
            case ColumnType.Int:
                return int.Parse(value);
            case ColumnType.Long:
                return long.Parse(value);
            default:
                return value;
        }
    }

    // decimal comma is supported alongside the invariant decimal point
    private static CultureInfo GetNumberCulture(string value)
    {
        return value.Contains(',') ? CultureInfo.GetCultureInfo("fi-FI") : CultureInfo.InvariantCulture;
    }
}
