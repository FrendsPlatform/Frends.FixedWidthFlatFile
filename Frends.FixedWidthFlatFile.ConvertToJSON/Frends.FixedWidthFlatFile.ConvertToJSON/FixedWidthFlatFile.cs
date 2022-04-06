using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

#pragma warning disable 1591

namespace Frends.FixedWidthFlatFile.ConvertToJSON
{
    public static class FixedWidthFlatFile
    {
        public static Result ParseJSON(List<Dictionary<string, dynamic>> data, string cultureInfo = null)
        {
            CultureInfo culture = string.IsNullOrWhiteSpace(cultureInfo) ? CultureInfo.InvariantCulture : new CultureInfo(cultureInfo);
            Lazy<JToken> jToken = new Lazy<JToken>(() => WriteToJToken(data, culture));
            return new Result { Data = JsonConvert.SerializeObject(jToken.Value) };
        }

        private static JToken WriteToJToken(List<Dictionary<string, dynamic>> data, CultureInfo culture)
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
        }
    }
}