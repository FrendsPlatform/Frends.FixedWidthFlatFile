using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Frends.FixedWidthFlatFile.CreateFromJson.Definitions;
using Frends.FixedWidthFlatFile.CreateFromJson.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.FixedWidthFlatFile.CreateFromJson;

/// <summary>
/// Task Class for FixedWidthFlatFile operations.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Converts a JSON array of objects to fixed-width records.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-FixedWidthFlatFile-CreateFromJson)
    /// </summary>
    /// <param name="input">JSON data, column layout, and formatting settings.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string FlatFileContent, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result CreateFromJson(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);
            cancellationToken.ThrowIfCancellationRequested();

            var culture = string.IsNullOrWhiteSpace(input.Culture)
                ? CultureInfo.InvariantCulture
                : CultureInfo.GetCultureInfo(input.Culture);
            ValidateColumns(input.ColumnSpecifications, cancellationToken);

            var destinationPath = options.DestinationType == DataLocation.File
                ? ResolveDestinationPath(options.DestinationFilePath)
                : null;

            var jsonText = input.SourceType == DataLocation.File
                ? ReadSourceFile(input.JsonFilePath)
                : input.Json;

            using var jsonReader = new JsonTextReader(new StringReader(jsonText));
            jsonReader.DateParseHandling = DateParseHandling.None;
            var json = JToken.ReadFrom(jsonReader);
            if (jsonReader.Read())
                throw new InvalidDataException("JSON input contains content after the root value.");
            if (json is not JArray rows)
                throw new InvalidDataException("JSON input must be an array of objects.");
            if (rows.Count == 0)
                throw new InvalidDataException("JSON input must contain at least one object.");

            var records = new List<string>(rows.Count + (input.IncludeHeaderRow ? 1 : 0));
            if (input.IncludeHeaderRow)
                records.Add(FormatHeader(input.ColumnSpecifications, cancellationToken));

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (row is not JObject jsonObject)
                    throw new InvalidDataException("Every item in the JSON array must be an object.");

                records.Add(FormatRecord(jsonObject, input.ColumnSpecifications, culture, cancellationToken));
            }

            var content = string.Join(Environment.NewLine, records);
            cancellationToken.ThrowIfCancellationRequested();

            if (destinationPath == null)
            {
                return new Result
                {
                    Success = true,
                    FlatFileContent = content,
                    FlatFilePath = null,
                    Error = null,
                };
            }

            WriteNewFile(destinationPath, content);
            return new Result
            {
                Success = true,
                FlatFileContent = null,
                FlatFilePath = destinationPath,
                Error = null,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string ReadSourceFile(string path)
    {
        return !File.Exists(path) ? throw new FileNotFoundException($"JSON source file '{path}' was not found.", path) : File.ReadAllText(path);
    }

    private static string ResolveDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Destination file path is not a valid path.");
        if (!Path.IsPathRooted(path))
            throw new ArgumentException("Destination file path must be an absolute path.");
        if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))
            throw new ArgumentException("Destination file path must point to a file, not a directory.");

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Destination file path is not a valid path: {ex.Message}");
        }

        if (string.IsNullOrEmpty(Path.GetFileName(fullPath)))
            throw new ArgumentException("Destination file path must include a file name.");
        if (Directory.Exists(fullPath))
            throw new IOException($"Destination path '{fullPath}' is an existing directory.");
        return File.Exists(fullPath) ? throw new IOException($"Destination file '{fullPath}' already exists and will not be overwritten.") : fullPath;
    }

    private static void WriteNewFile(string fullPath, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // CreateNew guarantees an existing file is never overwritten, even if it appeared after validation.
        using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static void ValidateColumns(IEnumerable<ColumnSpecification> columns, CancellationToken cancellationToken)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var column in columns)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (column == null)
                throw new InvalidDataException("Column specifications cannot contain null entries.");
            if (string.IsNullOrWhiteSpace(column.Name))
                throw new InvalidDataException("Every column must have a property name.");
            if (column.Length <= 0)
                throw new InvalidDataException($"Column '{column.Name}' must have a length greater than zero.");
            if (!names.Add(column.Name))
                throw new InvalidDataException($"Column name '{column.Name}' is defined more than once.");
        }
    }

    private static string FormatHeader(IEnumerable<ColumnSpecification> columns, CancellationToken cancellationToken)
    {
        var fields = new List<string>();
        foreach (var column in columns)
        {
            cancellationToken.ThrowIfCancellationRequested();
            fields.Add(PadField(column.Name, column));
        }

        return string.Concat(fields);
    }

    private static string FormatRecord(
        JObject jsonObject,
        IEnumerable<ColumnSpecification> columns,
        CultureInfo culture,
        CancellationToken cancellationToken)
    {
        var fields = new List<string>();
        foreach (var column in columns)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var token = jsonObject.GetValue(column.Name, StringComparison.Ordinal);
            var value = FormatValue(token, column, culture);
            fields.Add(PadField(value, column));
        }

        return string.Concat(fields);
    }

    private static string FormatValue(JToken token, ColumnSpecification column, CultureInfo culture)
    {
        if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            return string.Empty;

        switch (column.Type)
        {
            case ColumnType.String:
                return token.Type != JTokenType.String ? throw new InvalidDataException($"Property '{column.Name}' must be a string.") : token.Value<string>();
            case ColumnType.Char:
                if (token.Type != JTokenType.String || token.Value<string>().Length != 1)
                    throw new InvalidDataException($"Property '{column.Name}' must contain exactly one character.");
                return token.Value<string>();
            case ColumnType.Int:
                return token.Type == JTokenType.String
                    ? int.Parse(token.Value<string>(), NumberStyles.Integer, culture).ToString(culture)
                    : token.Value<int>().ToString(culture);
            case ColumnType.Long:
                return token.Type == JTokenType.String
                    ? long.Parse(token.Value<string>(), NumberStyles.Integer, culture).ToString(culture)
                    : token.Value<long>().ToString(culture);
            case ColumnType.Decimal:
                return token.Type == JTokenType.String
                    ? decimal.Parse(token.Value<string>(), NumberStyles.Number, culture).ToString(culture)
                    : token.Value<decimal>().ToString(culture);
            case ColumnType.Double:
                return token.Type == JTokenType.String
                    ? double.Parse(token.Value<string>(), NumberStyles.Float | NumberStyles.AllowThousands, culture)
                        .ToString(culture)
                    : token.Value<double>().ToString(culture);
            case ColumnType.Boolean:
                return token.Value<bool>().ToString(culture);
            case ColumnType.DateTime:
                {
                    var date = token.Type == JTokenType.Date
                        ? token.Value<DateTime>()
                        : DateTime.Parse(token.Value<string>(), culture, DateTimeStyles.RoundtripKind);
                    return date.ToString(column.DateTimeFormat, culture);
                }

            default:
                throw new InvalidDataException($"Column '{column.Name}' has an unsupported type.");
        }
    }

    private static string PadField(string value, ColumnSpecification column)
    {
        if (value.Length > column.Length)
        {
            throw new InvalidDataException(
                $"Value for column '{column.Name}' is {value.Length} characters long, exceeding its width of {column.Length}.");
        }

        return value.PadRight(column.Length);
    }
}
