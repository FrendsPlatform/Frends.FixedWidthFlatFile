using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using Frends.FixedWidthFlatFile.ConvertToXML.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

#pragma warning disable 1591

namespace Frends.FixedWidthFlatFile.ConvertToXML;

/// <summary>
/// Container class for Frends.FixedWidthFlatFile.ConvertToXML Task.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task converts List&lt;dictionary&lt;string, dynamic&gt;&gt; typed object to xml string. Mainly used to convert Frends.FixedWidthFlatFile.Parse result object to xml string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FixedWidthFlatFile.ConvertToXML)
    /// </summary>
    /// <param name="input">What value to convert.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { bool Success, Error Error, string Data }</returns>
    public static Result ConvertToXML([PropertyTab] Input input, [PropertyTab] Options options, CancellationToken cancellationToken)
    {
        try
        {
            if (input.FileContent == null || input.FileContent.Count <= 0)
                throw new ArgumentNullException(nameof(input), "FileContent not given. Cannot be empty.");

            var xml = WriteToXmlString(input.FileContent, cancellationToken);
            if (xml == null) throw new Exception("XML parse failed.");

            return new Result(true, xml);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string WriteToXmlString(List<Dictionary<string, dynamic>> data, CancellationToken cancellationToken)
    {
        try
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(ms, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
                {
                    writer.WriteStartDocument(); // start doc
                    writer.WriteStartElement("Root");
                    writer.WriteStartElement("Rows");

                    foreach (var row in data)
                    {
                        writer.WriteStartElement("Row");

                        foreach (var key in row.Keys)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            // value null check
                            if (row[key] != null)
                            {
                                if (row[key].GetType() == typeof(DateTime))
                                    writer.WriteElementString(key, row[key].ToString("s"));
                                else
                                    writer.WriteElementString(key, row[key].ToString());
                            }
                            else // write empty string for null values
                                writer.WriteElementString(key, "");
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
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
