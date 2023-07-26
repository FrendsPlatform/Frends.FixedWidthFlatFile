using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;



namespace Frends.FixedWidthFlatFile.ConvertToJSON;

/// <summary>
/// Task's main class.
/// </summary>
public static class FixedWidthFlatFile
{
    /// <summary>
    /// Task converts List&lt;dictionary&lt;string, dynamic&gt;&gt; typed object to json string. Mainly used to convert Frends.FixedWidthFlatFile.Parse result object to json string.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FixedWidthFlatFile.ConvertToJSON)
    /// </summary>
    /// <param name="data">Value to convert.</param>
    /// <param name="cancellationToken">CancellationToken given by Frends.</param>
    /// <returns>object { string Data }</returns>
    public static Result ConvertToJSON([PropertyTab] Input data, CancellationToken cancellationToken)
    {
        if (data.FileContent == null || data.FileContent.Count <= 0) throw new ArgumentNullException("FileContent not given or in wrong type. Cannot be empty.");

        CultureInfo culture = string.IsNullOrWhiteSpace(data.culture) ? CultureInfo.InvariantCulture : new CultureInfo(data.culture);
        JToken jToken = WriteToJToken(data.FileContent, culture, cancellationToken);
        return jToken != null ? new Result(JsonConvert.SerializeObject(jToken)) : throw new Exception("JSON parse failed.");
    }

    private static JToken WriteToJToken(List<Dictionary<string, dynamic>> data, CultureInfo culture, CancellationToken cancellationToken)
    {
        try
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
                        else //write empty string value for null fields
                            writer.WriteValue("");
                    }

                    writer.WriteEndObject(); // end row
                }

                writer.WriteEndArray(); // root array end

                return writer.Token;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}