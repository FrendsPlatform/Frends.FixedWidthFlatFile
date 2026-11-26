using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromObject.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Column layout used to format each row. Names identify values in the data; blank names use Field_1, Field_2, and so on.
    /// </summary>
    /// <example>[{ "Name": "Name", "Type": "String", "Length": 8 }]</example>
    [Required]
    [MinLength(1)]
    public ColumnSpecification[] ColumnSpecifications { get; set; } = [];

    /// <summary>
    /// Whether to write a header row, and how to format it.
    /// </summary>
    /// <example>HeaderRowType.FixedWidth</example>
    [DefaultValue(HeaderRowType.FixedWidth)]
    public HeaderRowType HeaderRow { get; set; } = HeaderRowType.FixedWidth;

    /// <summary>
    /// Delimiter used between column names when HeaderRow is Delimited.
    /// </summary>
    /// <example>;</example>
    [UIHint(nameof(HeaderRow), "", HeaderRowType.Delimited)]
    [RequiredIf(nameof(HeaderRow), HeaderRowType.Delimited)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string HeaderDelimiter { get; set; } = string.Empty;

    /// <summary>
    /// Enables skipping data rows before writing the file.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool SkipRows { get; set; }

    /// <summary>
    /// Number of data rows to skip from the beginning.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int SkipRowsFromTop { get; set; }

    /// <summary>
    /// Number of data rows to skip from the end, after skipping from the beginning.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int SkipRowsFromBottom { get; set; }

    /// <summary>
    /// Whether generated content is returned as a string or written to a file.
    /// </summary>
    /// <example>DataLocation.String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation DestinationType { get; set; } = DataLocation.String;

    /// <summary>
    /// Absolute path of the new output file. Missing directories are created; existing files are not overwritten.
    /// </summary>
    /// <example>C:\output\data.txt</example>
    [UIHint(nameof(DestinationType), "", DataLocation.File)]
    [RequiredIf(nameof(DestinationType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string DestinationFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to throw an error when the task fails. When false, failure details are returned in the Result.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Optional context prefixed to the error message when the task fails.
    /// </summary>
    /// <example>Custom error message</example>
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
