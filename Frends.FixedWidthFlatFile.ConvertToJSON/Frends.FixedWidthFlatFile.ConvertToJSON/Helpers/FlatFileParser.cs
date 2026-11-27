using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;

/// <summary>
/// Parses fixed width flat file content.
/// </summary>
internal static class FlatFileParser
{
    internal static List<Dictionary<string, dynamic>> Parse(
        string content,
        Options options,
        CultureInfo culture,
        CancellationToken cancellationToken)
    {
        var headers = new List<string>();
        var outputData = new List<Dictionary<string, dynamic>>();

        List<string> inputRows;
        using (var reader = new StringReader(content))
        {
            inputRows = ReadLinesToList(reader, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        switch (options.HeaderRow)
        {
            case HeaderRowType.Delimited:
                char headerDelimiter = Convert.ToChar(options.HeaderDelimiter);
                headers = SplitToList(inputRows.First(), headerDelimiter);
                inputRows.RemoveAt(0);
                break;
            case HeaderRowType.FixedWidth:
                headers = SplitToList(inputRows.First(), options.ColumnSpecifications);
                inputRows.RemoveAt(0);
                break;
        }

        // add header values as name if not set in column specification
        for (var i = 0; i < headers.Count && i < options.ColumnSpecifications.Length; i++)
        {
            if (string.IsNullOrEmpty(options.ColumnSpecifications[i].Name))
                options.ColumnSpecifications[i].Name = headers[i];
        }

        if (options.SkipRows)
        {
            if (options.SkipRowsFromTop > 0)
            {
                inputRows = options.SkipRowsFromTop >= inputRows.Count
                    ? new List<string>()
                    : inputRows.Skip(options.SkipRowsFromTop).ToList();
            }

            if (options.SkipRowsFromBottom > 0)
            {
                inputRows = options.SkipRowsFromBottom >= inputRows.Count
                    ? new List<string>()
                    : inputRows.Take(inputRows.Count - options.SkipRowsFromBottom).ToList();
            }
        }

        foreach (var dataRow in inputRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            outputData.Add(ParseDataRow(dataRow, options.ColumnSpecifications, culture));
        }

        return outputData;
    }

    private static List<string> ReadLinesToList(StringReader reader, CancellationToken cancellationToken)
    {
        var allLines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // skip empty lines
            if (!string.IsNullOrWhiteSpace(line))
                allLines.Add(line);
        }

        return allLines;
    }

    private static List<string> SplitToList(
        string row,
        char delimiter)
    {
        return
        [
            .. row
                .Split([delimiter], StringSplitOptions.None)
        ];
    }

    private static List<string> SplitToList(
        string row,
        ColumnSpecification[] columnSpecifications,
        bool rejectTrailingContent = false)
    {
        try
        {
            var values = new List<string>();

            int startIndex = 0;
            foreach (var columnSpec in columnSpecifications)
            {
                var value = row.Substring(startIndex, columnSpec.Length);
                values.Add(value.Trim());

                startIndex += columnSpec.Length;
            }

            if (rejectTrailingContent && !string.IsNullOrWhiteSpace(row.Substring(startIndex)))
                throw new InvalidDataException("Data row is longer than the column specifications.");

            return values;
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Data row did not match column specifications.", ex);
        }
    }

    private static Dictionary<string, dynamic> ParseDataRow(
        string row,
        ColumnSpecification[] columnSpecifications,
        CultureInfo culture)
    {
        var parsedData = new Dictionary<string, dynamic>();

        var rowValues = SplitToList(row, columnSpecifications, true);

        for (var i = 0; i < rowValues.Count; i++)
        {
            var columnSpec = columnSpecifications[i];
            var columnValue = rowValues[i];
            var columnName = columnSpec.Name;
            if (string.IsNullOrEmpty(columnName))
                columnName = $"Field_{i + 1}";

            if (string.IsNullOrWhiteSpace(columnValue))
            {
                parsedData.AddKeyValuePair(columnName, null);
                continue;
            }

            switch (columnSpec.Type)
            {
                case ColumnType.Boolean:
                    parsedData.AddKeyValuePair(columnName, bool.Parse(columnValue));
                    break;
                case ColumnType.Char:
                    parsedData.AddKeyValuePair(columnName, char.Parse(columnValue));
                    break;
                case ColumnType.DateTime:
                    var dateTime = string.IsNullOrEmpty(columnSpec.DateTimeFormat)
                        ? DateTime.Parse(columnValue, culture)
                        : DateTime.ParseExact(columnValue, columnSpec.DateTimeFormat, culture);
                    parsedData.AddKeyValuePair(columnName, dateTime);
                    break;
                case ColumnType.Decimal:
                    parsedData.AddKeyValuePair(columnName, decimal.Parse(columnValue, culture));
                    break;
                case ColumnType.Double:
                    var number = double.Parse(columnValue, culture);
                    if (!double.IsFinite(number))
                        throw new FormatException($"Value '{columnValue}' is not a finite number.");
                    parsedData.AddKeyValuePair(columnName, number);
                    break;
                case ColumnType.Int:
                    parsedData.AddKeyValuePair(columnName, int.Parse(columnValue, culture));
                    break;
                case ColumnType.Long:
                    parsedData.AddKeyValuePair(columnName, long.Parse(columnValue, culture));
                    break;
                default:
                    parsedData.AddKeyValuePair(columnName, columnValue);
                    break;
            }
        }

        return parsedData;
    }

    private static void AddKeyValuePair(
        this Dictionary<string, dynamic> dictionary,
        string key,
        dynamic value)
    {
        var originalKey = key;
        var renameIndex = 1;
        while (dictionary.ContainsKey(key))
        {
            key = $"{originalKey}_{renameIndex}";
            renameIndex++;
        }

        dictionary.Add(key, value);
    }
}
