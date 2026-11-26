using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Frends.FixedWidthFlatFile.CreateFromXml.Definitions;

namespace Frends.FixedWidthFlatFile.CreateFromXml.Helpers;

internal static class FlatFileWriter
{
    internal static string Write(XDocument document, Options options, CancellationToken cancellationToken)
    {
        var rows = document.Root?.Element("Rows")?.Elements("Row").ToList()
                   ?? throw new InvalidDataException("XML does not match the expected Root/Rows/Row structure.");

        var specs = options.ColumnSpecifications;
        var culture = string.IsNullOrWhiteSpace(options.Culture)
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(options.Culture.Trim(), predefinedOnly: true);

        var lines = new List<string>(rows.Count + 1);

        switch (options.HeaderRow)
        {
            case HeaderRowType.FixedWidth:
                lines.Add(BuildFixedWidthLine(HeaderNames(specs, rows), specs, 0));
                break;
            case HeaderRowType.Delimited:
            {
                var header = string.Join(options.HeaderDelimiter, HeaderNames(specs, rows));
                if (header.Contains('\r') || header.Contains('\n'))
                    throw new InvalidDataException("Header row contains a line break.");
                lines.Add(header);
                break;
            }
        }

        for (var r = 0; r < rows.Count; r++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var elements = rows[r].Elements().ToList();
            var values = new string[specs.Length];
            for (var i = 0; i < specs.Length; i++)
                values[i] = FormatValue(FindElement(elements, specs[i], i)?.Value, specs[i], culture, i);

            lines.Add(BuildFixedWidthLine(values, specs, r + 1));
        }

        return string.Join(Environment.NewLine, lines);
    }

    internal static XDocument ReadXml(TextReader reader)
    {
        using var xmlReader = XmlReader.Create(reader, SafeSettings());
        return XDocument.Load(xmlReader);
    }

    internal static XDocument ReadXml(Stream stream)
    {
        using var xmlReader = XmlReader.Create(stream, SafeSettings());
        return XDocument.Load(xmlReader);
    }

    // DTD processing is prohibited to protect against XXE and entity expansion attacks.
    private static XmlReaderSettings SafeSettings() =>
        new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

    private static string[] HeaderNames(ColumnSpecification[] specs, List<XElement> rows)
    {
        var firstRowElements = rows.Count > 0 ? rows[0].Elements().ToList() : [];
        var names = new string[specs.Length];
        for (var i = 0; i < specs.Length; i++)
        {
            names[i] = !string.IsNullOrEmpty(specs[i].Name)
                ? specs[i].Name
                : i < firstRowElements.Count ? firstRowElements[i].Name.LocalName : string.Empty;
        }

        return names;
    }

    private static XElement FindElement(List<XElement> elements, ColumnSpecification spec, int index)
    {
        if (!string.IsNullOrEmpty(spec.Name))
            return elements.FirstOrDefault(e => e.Name.LocalName == spec.Name);

        return index < elements.Count ? elements[index] : null;
    }

    private static string BuildFixedWidthLine(string[] values, ColumnSpecification[] specs, int rowNumber)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < specs.Length; i++)
        {
            var value = values[i] ?? string.Empty;
            if (value.Length > specs[i].Length)
            {
                throw new InvalidDataException(
                    $"Value '{value}' of column {i + 1} on line {rowNumber + 1} is longer than the column length {specs[i].Length}.");
            }

            if (value.Contains('\r') || value.Contains('\n'))
            {
                throw new InvalidDataException(
                    $"Value of column {i + 1} on line {rowNumber + 1} contains a line break.");
            }

            sb.Append(value.PadRight(specs[i].Length));
        }

        return sb.ToString();
    }

    private static string FormatValue(string text, ColumnSpecification spec, CultureInfo culture, int index)
    {
        // Whitespace is meaningful content for textual columns.
        var textual = spec.Type is ColumnType.String or ColumnType.Char;
        if (string.IsNullOrEmpty(text) || (!textual && string.IsNullOrWhiteSpace(text)))
            return string.Empty;

        var inv = CultureInfo.InvariantCulture;
        try
        {
            return spec.Type switch
            {
                ColumnType.Boolean => bool.Parse(text.Trim()).ToString(),
                ColumnType.Char => char.Parse(text).ToString(),
                ColumnType.DateTime => FormatDateTime(text.Trim(), spec, culture),
                ColumnType.Decimal => decimal.Parse(text.Trim(), inv).ToString(culture),
                ColumnType.Double => double.Parse(text.Trim(), inv).ToString(culture),
                ColumnType.Int => int.Parse(text.Trim(), inv).ToString(culture),
                ColumnType.Long => long.Parse(text.Trim(), inv).ToString(culture),
                _ => text,
            };
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException(
                $"Value '{text}' of column {index + 1} cannot be converted to {spec.Type}.",
                ex);
        }
    }

    private static string FormatDateTime(string text, ColumnSpecification spec, CultureInfo culture)
    {
        var date = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        var format = spec.DateTimeFormat;
        return date.ToString(format, culture);
    }
}
