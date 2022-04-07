using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;

#pragma warning disable 1591

namespace Frends.FixedWidthFlatFile.ConvertToJSON
{
    public static class FixedWidthFlatFile
    {
        /// <summary>
        /// This is a task that converts given value to json string.
        /// Documentation: https://github.com/CommunityHiQ/Frends.FixedWidthFlatFile.ConvertToJSON
        /// </summary>
        /// <param name="input">What value to convert.</param>
        /// <returns>{string Data} </returns>
        public static Result ParseJSON(Input data)
        {
            if(data.FileContent == null || data.FileContent.Count <= 0) throw new ArgumentNullException("FileContent not given. Cannot be empty.");

            CultureInfo culture = string.IsNullOrWhiteSpace(data.culture) ? CultureInfo.InvariantCulture : new CultureInfo(data.culture);
            Lazy<JToken> jToken = new Lazy<JToken>(() => WriteToJToken(data.FileContent, culture));
            return jToken.Value != null ? new Result (JsonConvert.SerializeObject(jToken.Value)) : throw new Exception("JSON parse failed.");
        }

        private static JToken WriteToJToken(List<Dictionary<string, dynamic>> data, CultureInfo culture)
        {
            try
            {
                using (var writer = new JTokenWriter())
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    writer.Culture = culture;

                    writer.WriteStartArray(); // root start

                    foreach (var row in data)
                    {
                        writer.WriteStartObject(); // start row object
                        foreach (var key in row.Keys)
                        {
                            writer.WritePropertyName(key);
                            // null check
                            if (row[key] != null)
                                writer.WriteValue(row[key]);
                            else //write empty string value for null fields
                                writer.WriteValue("");
                        }

                        writer.WriteEndObject(); // end row
                    }

                    writer.WriteEndArray(); // root array end

                    return writer.Token;
                }
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}