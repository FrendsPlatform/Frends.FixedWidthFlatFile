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
    /// <param name="input">Fixed width flat file content and its column specifications.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>Object { bool Success, Error Error, string Data }</returns>
    public static Result ConvertToXML([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input.FlatFileContent))
                throw new ArgumentException("FlatFileContent not given. Cannot be empty.");

            if (input.ColumnSpecifications == null || input.ColumnSpecifications.Length == 0)
                throw new ArgumentException("ColumnSpecifications not given. Cannot be empty.");

            var parsedFile = FlatFileParser.Parse(input, options, cancellationToken);

            return new Result(true, WriteToXmlString(parsedFile, cancellationToken));
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
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
