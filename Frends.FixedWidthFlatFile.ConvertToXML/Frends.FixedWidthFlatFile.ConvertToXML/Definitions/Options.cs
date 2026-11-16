using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    [DefaultValue(0)]
    public int SkipRowsFromTop { get; set; }

    /// <summary>
    /// Count of data rows to skip from bottom.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [DefaultValue(0)]
    public int SkipRowsFromBottom { get; set; }

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
