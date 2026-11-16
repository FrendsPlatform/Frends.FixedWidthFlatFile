using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.ConvertToXML.Attributes;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Whether the flat file is provided as a string or read from a file.
    /// </summary>
    /// <example>String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation SourceType { get; set; } = DataLocation.String;

    /// <summary>
    /// Fixed width flat file content in string format.
    /// </summary>
    /// <example>
    /// <code>
    /// Name    Street    StartDate
    /// Veijo   FrendsStr 20180527
    /// Hodor   HodorsStr 20180101
    /// </code> </example>
    [UIHint(nameof(SourceType), "", DataLocation.String)]
    [RequiredIf(nameof(SourceType), DataLocation.String)]
    [DisplayFormat(DataFormatString = "Text")]
    public string FlatFileContent { get; set; } = string.Empty;

    /// <summary>
    /// Path to a file containing the fixed width flat file content.
    /// </summary>
    /// <example>C:\input\data.txt</example>
    [UIHint(nameof(SourceType), "", DataLocation.File)]
    [RequiredIf(nameof(SourceType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string FlatFilePath { get; set; } = string.Empty;

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
    [RequiredIf(nameof(HeaderRow), HeaderRowType.Delimited)]
    [DisplayFormat(DataFormatString = "Text")]
    public string HeaderDelimiter { get; set; } = string.Empty;

    /// <summary>
    /// Column specifications for the data that is being converted.
    /// </summary>
    /// <example>[ Example, ColumnType.String, yyyy-MM-ddTHH:mm:ss, 5 ]</example>
    [Required]
    [MinLength(1, ErrorMessage = "ColumnSpecifications not given. Cannot be empty.")]
    public ColumnSpecification[] ColumnSpecifications { get; set; } = Array.Empty<ColumnSpecification>();
}
