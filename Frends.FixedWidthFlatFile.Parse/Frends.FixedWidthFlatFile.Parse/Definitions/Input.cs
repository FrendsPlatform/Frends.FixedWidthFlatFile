using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Parse Task input specification.
/// </summary>
public class Input
{
    /// <summary>
    /// Fixed width flat file content.
    /// </summary>
    /// <example>Example</example>
    [DisplayFormat(DataFormatString = "Expression")]
    public string FlatFileContent { get; set; } = string.Empty;

    /// <summary>
    /// None: Flat file does not contain header row
    /// FixedWidth: Header row is parsed using column specification
    /// Delimited: Header row is parsed using delimiter char
    /// </summary>
    /// <example>HeaderRowType.FixedWidth</example>
    [DefaultValue(HeaderRowType.FixedWidth)]
    public HeaderRowType HeaderRow { get; set; }

    /// <summary>
    /// If header row uses delimiter set it here.
    /// </summary>
    /// <example>;</example>
    [UIHint(nameof(HeaderRow), "", HeaderRowType.Delimited)]
    [DisplayFormat(DataFormatString = "Text")]
    public string HeaderDelimiter { get; set; } = string.Empty;

    /// <summary>
    /// Column specifications for the data that is being parsed.
    /// </summary>
    /// <example>[ Example, ColumnType.String, yyyy-MM-ddTHH:mm:ss, 5 ]</example>
    public ColumnSpecification[] ColumnSpecifications { get; set; } = new ColumnSpecification[0];
}