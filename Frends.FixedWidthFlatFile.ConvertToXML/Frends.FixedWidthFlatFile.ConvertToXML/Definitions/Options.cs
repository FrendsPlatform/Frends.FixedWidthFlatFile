using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.ConvertToXML.Attributes;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Enables skipping of data rows.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool SkipRows { get; set; }

    /// <summary>
    /// Count of data rows to skip from top.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int SkipRowsFromTop { get; set; }

    /// <summary>
    /// Count of data rows to skip from bottom.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int SkipRowsFromBottom { get; set; }

    /// <summary>
    /// Whether the XML is returned as a string or written to a file.
    /// </summary>
    /// <example>String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation DestinationType { get; set; } = DataLocation.String;

    /// <summary>
    /// Full path of the XML file to create. Missing directories are created; an existing file is never overwritten.
    /// </summary>
    /// <example>C:\output\data.xml</example>
    [UIHint(nameof(DestinationType), "", DataLocation.File)]
    [RequiredIf(nameof(DestinationType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string DestinationFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure. If `ThrowErrorOnFailure` is set to `true`, then the original exception will be wrapped in a new Exception with this error message.
    /// </summary>
    /// <example>Converting file content to XML failed.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
