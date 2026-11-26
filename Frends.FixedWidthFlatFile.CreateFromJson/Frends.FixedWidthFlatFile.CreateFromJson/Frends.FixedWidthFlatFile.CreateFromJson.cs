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

            var destinationPath = options.DestinationType == DataLocation.File
                ? ResolveDestinationPath(options.DestinationFilePath)
                : null;

            var jsonText = input.SourceType == DataLocation.File
                ? ReadSourceFile(input.JsonFilePath)
                : input.Json;

            var json = ParseJson(jsonText);
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

    private static JToken ParseJson(string jsonText)
    {
        try
        {
            // Decimal parsing keeps full precision of JSON numbers for Decimal columns.
            return ReadJson(jsonText, FloatParseHandling.Decimal);
        }
        catch (JsonReaderException)
        {
            // Numbers outside the decimal range are still supported as doubles.
            return ReadJson(jsonText, FloatParseHandling.Double);
        }
    }

    private static JToken ReadJson(string jsonText, FloatParseHandling floatParseHandling)
    {
        using var jsonReader = new JsonTextReader(new StringReader(jsonText));
        jsonReader.DateParseHandling = DateParseHandling.None;
        jsonReader.FloatParseHandling = floatParseHandling;
        var json = JToken.ReadFrom(jsonReader);
        if (jsonReader.Read())
            throw new InvalidDataException("JSON input contains content after the root value.");
        return json;
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
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }

            // Moving without overwrite guarantees an existing file is never replaced, even if it appeared after validation.
            File.Move(tempPath, fullPath, false);
        }
        catch
        {
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Best-effort cleanup; the original failure is what matters.
            }

            throw;
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
        try
        {
            return ConvertValue(token, column, culture);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or InvalidCastException or ArgumentException)
        {
            throw new InvalidDataException($"Property '{column.Name}' has a value that is not a valid {column.Type}: {ex.Message}", ex);
        }
    }

    private static long ParseIntegral(JToken token, ColumnSpecification column, long min, long max)
    {
        long value;
        switch (token.Type)
        {
            case JTokenType.String:
                value = long.Parse(token.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture);
                break;
            case JTokenType.Integer:
                value = token.Value<long>();
                break;
            case JTokenType.Float:
                var number = token.Value<decimal>();
                if (number != decimal.Truncate(number))
                    throw new InvalidDataException($"Property '{column.Name}' must be a whole number.");
                value = (long)number;
                break;
            default:
                throw new InvalidDataException($"Property '{column.Name}' must be a whole number.");
        }

        return value < min || value > max ? throw new OverflowException("Value is outside the range of the column type.") : value;
    }

    private static bool IsNumber(JToken token) => token.Type is JTokenType.Integer or JTokenType.Float;

    private static string ConvertValue(JToken token, ColumnSpecification column, CultureInfo culture)
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
                return ((int)ParseIntegral(token, column, int.MinValue, int.MaxValue)).ToString(culture);
            case ColumnType.Long:
                return ParseIntegral(token, column, long.MinValue, long.MaxValue).ToString(culture);
            case ColumnType.Decimal:
                if (token.Type == JTokenType.String)
                    return decimal.Parse(token.Value<string>(), NumberStyles.Number, CultureInfo.InvariantCulture).ToString(culture);
                return IsNumber(token)
                    ? token.Value<decimal>().ToString(culture)
                    : throw new InvalidDataException($"Property '{column.Name}' must be a number.");
            case ColumnType.Double:
                if (token.Type == JTokenType.String)
                {
                    return double.Parse(token.Value<string>(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
                        .ToString(culture);
                }

                return IsNumber(token)
                    ? token.Value<double>().ToString(culture)
                    : throw new InvalidDataException($"Property '{column.Name}' must be a number.");
            case ColumnType.Boolean:
                if (token.Type == JTokenType.Boolean)
                    return token.Value<bool>().ToString(culture);
                return token.Type == JTokenType.String
                    ? bool.Parse(token.Value<string>()).ToString(culture)
                    : throw new InvalidDataException($"Property '{column.Name}' must be a boolean.");
            case ColumnType.DateTime:
                {
                    if (token.Type != JTokenType.String)
                        throw new InvalidDataException($"Property '{column.Name}' must be a date string.");
                    var date = DateTime.Parse(token.Value<string>(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    return date.ToString(column.DateTimeFormat, culture);
                }

            default:
                throw new InvalidDataException($"Column '{column.Name}' has an unsupported type.");
        }
    }

    private static string PadField(string value, ColumnSpecification column)
    {
        if (value.IndexOfAny(['\r', '\n']) >= 0)
            throw new InvalidDataException($"Value for column '{column.Name}' contains a line break, which would split the record.");
        if (value.Length > column.Length)
        {
            throw new InvalidDataException(
                $"Value for column '{column.Name}' is {value.Length} characters long, exceeding its width of {column.Length}.");
        }

        return value.PadRight(column.Length);
    }
}
