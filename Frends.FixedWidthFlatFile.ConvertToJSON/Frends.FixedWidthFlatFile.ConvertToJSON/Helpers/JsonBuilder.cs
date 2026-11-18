using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Helpers;

/// <summary>
/// Builds json from parsed flat file rows.
/// </summary>
internal static class JsonBuilder
{
    internal static string Build(
        List<Dictionary<string, dynamic>> data,
        CancellationToken cancellationToken)
    {
        var token = WriteToJToken(data, cancellationToken);
        return JsonConvert.SerializeObject(token);
    }

    private static JToken WriteToJToken(
        List<Dictionary<string, dynamic>> data,
        CancellationToken cancellationToken)
    {
        using var writer = new JTokenWriter();
        writer.Formatting = Formatting.Indented;
        writer.Culture = CultureInfo.InvariantCulture;

        writer.WriteStartArray(); // root start

        foreach (var row in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            writer.WriteStartObject(); // start row object
            foreach (var key in row.Keys)
            {
                cancellationToken.ThrowIfCancellationRequested();

                writer.WritePropertyName(key);

                // write empty string value for null fields
                writer.WriteValue(row[key] ?? string.Empty);
            }

            writer.WriteEndObject(); // end row
        }

        writer.WriteEndArray(); // root array end

        return writer.Token;
    }
}
