using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.ConvertToJSON.Attributes;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// Input parameters for the Task.
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
    /// Fixed width flat file content in string format. The header row uses the same column widths as data rows.
    /// The example uses column widths 6, 10 and 8 and the date format yyyyMMdd.
    /// </summary>
    /// <example>
    /// <code>
    /// Name  Street    Birthday
    /// Veijo FrendsStr 20180527
    /// Hodor HodorsStr 20180101
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
}
