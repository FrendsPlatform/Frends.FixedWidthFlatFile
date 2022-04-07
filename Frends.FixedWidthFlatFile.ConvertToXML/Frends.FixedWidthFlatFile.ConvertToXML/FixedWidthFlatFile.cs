using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

#pragma warning disable 1591

namespace Frends.FixedWidthFlatFile.ConvertToXML
{
    public static class FixedWidthFlatFile
    {
        /// <summary>
        /// This is a task that converts given value to json string.
        /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.FixedWidthFlatFile.ConvertToXML)
        /// </summary>
        /// <param name="data">What value to convert.</param>
        /// <returns>object { string Data }</returns>
        public static Result ConvertToJSON([PropertyTab] Input data, CancellationToken cancellationToken)
        {
            if(data.FileContent == null || data.FileContent.Count <= 0) throw new ArgumentNullException("FileContent not given. Cannot be empty.");

            string xml = WriteToXmlString(data.FileContent, cancellationToken);
            return xml != null ? new Result (xml) : throw new Exception("XML parse failed.");
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
                            cancellationToken.ThrowIfCancellationRequested();
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
                                {
                                    writer.WriteElementString(key, "");
                                }
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
}