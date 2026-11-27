using System.Globalization;
using Frends.FixedWidthFlatFile.ConvertToObject.Definitions;

namespace Frends.FixedWidthFlatFile.ConvertToObject.Helpers;

/// <summary>
/// Parses fixed width flat file content into rows of key-value pairs.
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
            inputRows = ReadLinesToList(reader);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var columnNames = options.ColumnSpecifications.Select(column => column.Name).ToArray();

        switch (options.HeaderRow)
        {
            case HeaderRowType.Delimited:
                var headerDelimiter = Convert.ToChar(options.HeaderDelimiter);
                headers = SplitToList(inputRows.First(), headerDelimiter);
                if (headers.Count > columnNames.Length)
                    throw new InvalidDataException($"Delimited header contains {headers.Count} columns, but only {columnNames.Length} column specifications were provided.");

                inputRows.RemoveAt(0);
                break;
            case HeaderRowType.FixedWidth:
                headers = SplitToList(inputRows.First(), options.ColumnSpecifications);
                inputRows.RemoveAt(0);
                break;
        }

        if (headers.Count > 0)
        {
            // add header values as name if not set in column specification
            var index = 0;
            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(columnNames[index]))
                    columnNames[index] = header;
                index++;
            }
        }

        if (options.SkipRows)
        {
            if (options.SkipRowsFromTop > 0)
            {
                if (options.SkipRowsFromTop >= inputRows.Count)
                    inputRows.Clear();
                else
                    inputRows = inputRows.Skip(options.SkipRowsFromTop).ToList();
            }

            if (options.SkipRowsFromBottom > 0)
            {
                if (options.SkipRowsFromBottom >= inputRows.Count)
                    inputRows.Clear();
                else
                    inputRows = inputRows.Take(inputRows.Count - options.SkipRowsFromBottom).ToList();
            }
        }

        for (var rowIndex = 0; rowIndex < inputRows.Count; rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            outputData.Add(ParseDataRowWithColumnNames(inputRows[rowIndex], options.ColumnSpecifications, columnNames, rowIndex + 1, culture));
        }

        return outputData;
    }

    internal static List<string> ReadLinesToList(StringReader reader)
    {
        var allLines = new List<string>();
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            // skip empty lines
            if (!string.IsNullOrWhiteSpace(line))
                allLines.Add(line);
        }

        return allLines;
    }

    internal static List<string> SplitToList(string row, char delimiter)
    {
        return row
            .Split([delimiter], StringSplitOptions.None)
            .ToList();
    }

    internal static List<string> SplitToList(string row, ColumnSpecification[] columnSpecifications)
    {
        try
        {
            var values = new List<string>();

            var startIndex = 0;
            foreach (var columnSpec in columnSpecifications)
            {
                var value = row.Substring(startIndex, columnSpec.Length);
                values.Add(value.Trim());

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

    internal static Dictionary<string, dynamic> ParseDataRowWithColumnNames(
        string row,
        ColumnSpecification[] columnSpecifications,
        IReadOnlyList<string> columnNames,
        int rowNumber,
        CultureInfo culture)
    {
        var parsedData = new Dictionary<string, dynamic>();

        var rowValues = SplitToList(row, columnSpecifications);

        for (var i = 0; i < rowValues.Count; i++)
        {
            var columnSpec = columnSpecifications[i];
            var columnValue = rowValues[i];
            var columnName = columnNames[i];
            if (string.IsNullOrEmpty(columnName))
                columnName = $"Field_{i + 1}";

            try
            {
                if (string.IsNullOrWhiteSpace(columnValue))
                {
                    parsedData.AddKeyValuePair(columnName, null);
                }
                else
                {
                    switch (columnSpec.Type)
                    {
                        case ColumnType.Boolean:
                            parsedData.AddKeyValuePair(columnName, bool.Parse(columnValue));
                            break;
                        case ColumnType.Char:
                            parsedData.AddKeyValuePair(columnName, char.Parse(columnValue));
                            break;
                        case ColumnType.DateTime:
                            parsedData.AddKeyValuePair(columnName, string.IsNullOrEmpty(columnSpec.DateTimeFormat) ? DateTime.Parse(columnValue, culture) : DateTime.ParseExact(columnValue, columnSpec.DateTimeFormat, culture));
                            break;
                        case ColumnType.Decimal:
                            parsedData.AddKeyValuePair(columnName, decimal.Parse(columnValue, culture));
                            break;
                        case ColumnType.Double:
                            parsedData.AddKeyValuePair(columnName, double.Parse(columnValue, culture));
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
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                throw new InvalidDataException($"Failed to parse data row {rowNumber}, column '{columnName}'.", ex);
            }
        }

        return parsedData;
    }

    internal static Dictionary<string, dynamic> ParseDataRow(string row, ColumnSpecification[] columnSpecifications, CultureInfo culture = null)
    {
        var columnNames = columnSpecifications.Select(column => column.Name).ToArray();
        return ParseDataRowWithColumnNames(row, columnSpecifications, columnNames, 1, culture ?? CultureInfo.InvariantCulture);
    }

    internal static void AddKeyValuePair(this Dictionary<string, dynamic> dictionary, string key, dynamic value)
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
