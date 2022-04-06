using System.ComponentModel;
using System.Globalization;
using Frends.FixedWidthFlatFile.Parse.Definitions;

namespace Frends.FixedWidthFlatFile.Parse;

public static class FixedWidthFlatFile
{
    /// <summary>
    /// Parse Fixed Width data to object. [Documentation](https://tasks.frends.com/#frends-tasks/Frends.FixedWidthFlatFile.Parse)
    /// </summary>
    /// <param name="input">Input definition.</param>
    /// <param name="options">Additional input options.</param>
    /// <returns>Object { List&lt;Dictionary&lt;string Key, dynamic Value&gt;&gt; Data }</returns>
    public static Result Parse([PropertyTab] Input input, [PropertyTab] Options options)
    {
        var inputRows = new List<string>();
        var headers = new List<string>();
        var outputData = new List<Dictionary<string, dynamic>>();

        //== Read input data ==/
        using (var reader = new StringReader(input.FlatFileContent))
        {
            inputRows = ReadLinesToList(reader);
        }

        //== Parse header row ==/
        switch (input.HeaderRow)
        {
            case HeaderRowType.Delimited:
                char headerDelimiter = Convert.ToChar(input.HeaderDelimiter);
                headers = SplitToList(inputRows.First(), headerDelimiter);
                inputRows.RemoveAt(0);
                break;
            case HeaderRowType.FixedWidth:
                headers = SplitToList(inputRows.First(), input.ColumnSpecifications);
                inputRows.RemoveAt(0);
                break;
        }
        if (headers.Count > 0)
        {
            // add header values as name if not set in column specification
            var index = 0;
            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(input.ColumnSpecifications[index].Name))
                    input.ColumnSpecifications[index].Name = header;
                index++;
            }
        }

        //== Parse data rows ==/

        // Skip rows?
        if (options.SkipRows)
        {
            if (options.SkipRowsFromTop > 0)
            {
                // skipping more rows that exist?
                if (options.SkipRowsFromTop >= inputRows.Count)
                    inputRows.Clear();
                else
                    inputRows = inputRows.Skip(options.SkipRowsFromTop).ToList();
            }
            if (options.SkipRowsFromBottom > 0)
            {
                // skipping more rows that exist?
                if (options.SkipRowsFromBottom >= inputRows.Count)
                    inputRows.Clear();
                else
                    inputRows = inputRows.Take((inputRows.Count - options.SkipRowsFromBottom)).ToList();
            }
        }
        //== Process data rows ==/
        foreach (var dataRow in inputRows)
        {
            outputData.Add(ParseDataRow(dataRow, input.ColumnSpecifications));
        }

        return new Result { Data = outputData };
    }

    internal static List<string> ReadLinesToList(StringReader reader)
    {
        var allLines = new List<string>();
        string line;
        while (null != (line = reader.ReadLine()))
        {
            //skip empty lines
            if (!string.IsNullOrWhiteSpace(line))
                allLines.Add(line);
        }

        return allLines;
    }

    internal static List<string> SplitToList(string row, char delimiter)
    {
        return row
            .Split(new[] { delimiter }, StringSplitOptions.None)
            .ToList();
    }

    internal static List<string> SplitToList(string row, ColumnSpecification[] columnSpecifications)
    {
        try
        {
            var values = new List<string>();

            int startIndex = 0;
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

    internal static Dictionary<string, dynamic> ParseDataRow(string row, ColumnSpecification[] columnSpecifications)
    {
        var parsedData = new Dictionary<string, dynamic>();

        var rowValues = SplitToList(row, columnSpecifications);

        for (var i = 0; i < rowValues.Count; i++)
        {
            var columnSpec = columnSpecifications[i];
            var columnValue = rowValues[i];
            var columnName = columnSpec.Name;
            if (string.IsNullOrEmpty(columnName))
                columnName = $"Field_{i + 1}";

            if (string.IsNullOrWhiteSpace(columnValue))
                parsedData.AddKeyValuePair(columnSpec.Name, null);
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
                        parsedData.AddKeyValuePair(columnName, string.IsNullOrEmpty(columnSpec.DateTimeFormat) ?
                            DateTime.Parse(columnValue) :
                            DateTime.ParseExact(columnValue, columnSpec.DateTimeFormat, CultureInfo.InvariantCulture));
                        break;
                    case ColumnType.Decimal:
                        var cultureDecimal = columnValue.Contains(",") ? CultureInfo.GetCultureInfo("fi-FI") : CultureInfo.InvariantCulture;
                        parsedData.AddKeyValuePair(columnName, decimal.Parse(columnValue, cultureDecimal));
                        break;
                    case ColumnType.Double:
                        var cultureDouble = columnValue.Contains(",") ? CultureInfo.GetCultureInfo("fi-FI") : CultureInfo.InvariantCulture;
                        parsedData.AddKeyValuePair(columnName, double.Parse(columnValue, cultureDouble));
                        break;
                    case ColumnType.Int:
                        parsedData.AddKeyValuePair(columnName, int.Parse(columnValue));
                        break;
                    case ColumnType.Long:
                        parsedData.AddKeyValuePair(columnName, long.Parse(columnValue));
                        break;
                    default:
                        parsedData.AddKeyValuePair(columnName, columnValue);
                        break;
                }
            }
        }
        return parsedData;
    }

    /// <summary>
    /// Adds key and value. If key already exists, it is renamed with '_1', '_2' etc suffix.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    internal static void AddKeyValuePair(this Dictionary<string, dynamic> dictionary, string key, dynamic value)
    {
        var originalKey = key;
        int renameIndex = 1;
        while (dictionary.ContainsKey(key))
        {
            key = $"{originalKey}_{renameIndex.ToString()}";
            renameIndex++;
        }
        dictionary.Add(key, value);
    }
}
