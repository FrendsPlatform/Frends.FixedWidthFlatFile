using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.ConvertToObject.Attributes;

namespace Frends.FixedWidthFlatFile.ConvertToObject.Definitions;

/// <summary>
/// Essential parameters.
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
    /// Veijo FrendsStr 20180527
    /// Hodor HodorsStr 20180101
    /// </code> </example>
    [UIHint(nameof(SourceType), "", DataLocation.String)]
    [RequiredIf(nameof(SourceType), DataLocation.String)]
    [DisplayFormat(DataFormatString = "Text")]
    public string FlatFileContent { get; set; } = string.Empty;

    /// <summary>
    /// Path to an existing file containing the fixed width flat file content.
    /// </summary>
    /// <example>C:\input\data.txt</example>
    [UIHint(nameof(SourceType), "", DataLocation.File)]
    [RequiredIf(nameof(SourceType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string FlatFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Culture used to parse numeric and date values. When empty, invariant culture is used.
    /// </summary>
    /// <example>fi-FI</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Culture { get; set; } = string.Empty;
}
