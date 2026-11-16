using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using Frends.FixedWidthFlatFile.ConvertToXML.Helpers;

#pragma warning disable 1591

namespace Frends.FixedWidthFlatFile.ConvertToXML;

/// <summary>
/// Container class for Frends.FixedWidthFlatFile.ConvertToXML Task.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task converts fixed width flat file content to xml string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FixedWidthFlatFile.ConvertToXML)
    /// </summary>
    /// <param name="input">Fixed width flat file content or path.</param>
    /// <param name="options">Flat file structure and additional parameters.</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>Object { bool Success, Error Error, string Data, string FilePath }</returns>
    public static Result ConvertToXML([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, options);

            var destinationPath = options.DestinationType == DataLocation.File
                ? ResolveDestinationPath(options.DestinationFilePath)
                : null;

            var content = input.SourceType == DataLocation.File
                ? ReadSourceFile(input.FlatFilePath)
                : input.FlatFileContent;

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Flat file content is empty.");

            var parsedFile = FlatFileParser.Parse(content, options, cancellationToken);
            var xml = WriteToXmlString(parsedFile, cancellationToken);

            if (destinationPath == null)
                return new Result(true, xml);

            WriteNewFile(destinationPath, xml);
            return new Result(true, filePath: destinationPath);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string ReadSourceFile(string path)
    {
        return !File.Exists(path)
            ? throw new FileNotFoundException($"Flat file source '{path}' was not found.", path)
            : File.ReadAllText(path);
    }

    private static string ResolveDestinationPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new ArgumentException("Destination file path is not a valid path.");
        if (!Path.IsPathRooted(path))
            throw new ArgumentException("Destination file path must be an absolute path.");
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
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // CreateNew guarantees an existing file is never overwritten, even if it appeared after validation.
        using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string WriteToXmlString(ParsedFlatFile data, CancellationToken cancellationToken)
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new XmlTextWriter(ms, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
            {
                writer.WriteStartDocument(); // start doc
                writer.WriteStartElement("Root");
                writer.WriteStartElement("Rows");

                foreach (var row in data.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    writer.WriteStartElement("Row");

                    for (var i = 0; i < data.ColumnNames.Length; i++)
                    {
                        // write empty string for null values
                        writer.WriteElementString(data.ColumnNames[i], ValueToString(row[i]));
                    }

                    writer.WriteEndElement(); // end Row
                }

                writer.WriteEndElement(); // end Rows
                writer.WriteEndElement(); // end Root
                writer.WriteEndDocument(); // end doc
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    private static string ValueToString(object value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dateTime => dateTime.ToString("s", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };
    }
}
