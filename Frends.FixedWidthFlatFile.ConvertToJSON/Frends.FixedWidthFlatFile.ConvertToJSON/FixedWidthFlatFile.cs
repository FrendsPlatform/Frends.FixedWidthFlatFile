using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.FixedWidthFlatFile.ConvertToJSON;

/// <summary>
/// Task's main class.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task parses fixed width flat file content and converts it to a json string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FixedWidthFlatFile.ConvertToJSON)
    /// </summary>
    /// <param name="input">Flat file content and its specifications.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>object { bool Success, Error Error, string Data, string FilePath }</returns>
    public static Result ConvertToJSON([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);

            var destinationPath = options.DestinationType == DataLocation.File
                ? ResolveDestinationPath(options.DestinationFilePath)
                : null;

            var content = input.FlatFileContent;
            if (input.SourceType == DataLocation.File)
            {
                if (!File.Exists(input.FlatFilePath))
                    throw new FileNotFoundException($"Flat file source '{input.FlatFilePath}' was not found.", input.FlatFilePath);
                content = File.ReadAllText(input.FlatFilePath);
            }

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Flat file content is empty.");

            CultureInfo culture = string.IsNullOrWhiteSpace(options.Culture) ? CultureInfo.InvariantCulture : new CultureInfo(options.Culture);
            var data = Parse(content, options, cancellationToken);
            JToken jToken = WriteToJToken(data, culture, cancellationToken);
            if (jToken == null) throw new Exception("JSON parse failed.");

            var json = JsonConvert.SerializeObject(jToken);
            if (destinationPath == null)
                return new Result(true, json);

            WriteNewFile(destinationPath, json);
            return new Result(true, filePath: destinationPath);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string ResolveDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Destination file path is not a valid path.");
        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException("Destination file path must be a fully qualified absolute path.");
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
            throw new ArgumentException("Destination file path must point to a file, not a directory.");

        var fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
            throw new IOException($"Destination path '{fullPath}' is an existing directory.");
        return File.Exists(fullPath)
            ? throw new IOException($"Destination file '{fullPath}' already exists and will not be overwritten.")
            : fullPath;
    }

    private static void WriteNewFile(string fullPath, string content)
    {
        var directory = Path.GetDirectoryName(fullPath);
        Directory.CreateDirectory(directory);

        // Write to a temp file first so a failed write never leaves a partial file at the final path.
        var tempPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(content);
            }

            // Move without overwrite throws if the file appeared after validation.
            File.Move(tempPath, fullPath, false);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }

    internal static List<Dictionary<string, dynamic>> Parse(string content, Options options, CancellationToken cancellationToken)
    {
        var headers = new List<string>();
        var outputData = new List<Dictionary<string, dynamic>>();

        List<string> inputRows;
        using (var reader = new StringReader(content))
        {
            inputRows = ReadLinesToList(reader);
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
            outputData.Add(ParseDataRow(dataRow, options.ColumnSpecifications));
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

                startIndex += columnSpec.Length;
            }

            return values;
        }
        catch (Exception ex)
        {
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
                    parsedData.AddKeyValuePair(columnName, string.IsNullOrEmpty(columnSpec.DateTimeFormat) ? DateTime.Parse(columnValue) : DateTime.ParseExact(columnValue, columnSpec.DateTimeFormat, CultureInfo.InvariantCulture));
                    break;
                case ColumnType.Decimal:
                    var cultureDecimal = columnValue.Contains(',') ? CultureInfo.GetCultureInfo("fi-FI") : CultureInfo.InvariantCulture;
                    parsedData.AddKeyValuePair(columnName, decimal.Parse(columnValue, cultureDecimal));
                    break;
                case ColumnType.Double:
                    var cultureDouble = columnValue.Contains(',') ? CultureInfo.GetCultureInfo("fi-FI") : CultureInfo.InvariantCulture;
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

        return parsedData;
    }

    internal static void AddKeyValuePair(this Dictionary<string, dynamic> dictionary, string key, dynamic value)
    {
        var originalKey = key;
        int renameIndex = 1;
        while (dictionary.ContainsKey(key))
        {
            key = $"{originalKey}_{renameIndex}";
            renameIndex++;
        }

        dictionary.Add(key, value);
    }

    private static JToken WriteToJToken(List<Dictionary<string, dynamic>> data, CultureInfo culture, CancellationToken cancellationToken)
    {
        using (var writer = new JTokenWriter())
        {
            writer.Formatting = Formatting.Indented;
            writer.Culture = culture;

            writer.WriteStartArray(); // root start

            foreach (var row in data)
            {
                cancellationToken.ThrowIfCancellationRequested();

                writer.WriteStartObject(); // start row object
                foreach (var key in row.Keys)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    writer.WritePropertyName(key);

                    // null check
                    if (row[key] != null)
                        writer.WriteValue(row[key]);
                    else // write empty string value for null fields
                        writer.WriteValue(string.Empty);
                }

                writer.WriteEndObject(); // end row
            }

            writer.WriteEndArray(); // root array end

            return writer.Token;
        }
    }
}