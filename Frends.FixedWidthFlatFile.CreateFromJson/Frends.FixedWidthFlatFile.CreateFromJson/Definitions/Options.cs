using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromJson.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether the flat file is returned as a string or written to a file.
    /// </summary>
    /// <example>String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation DestinationType { get; set; } = DataLocation.String;

    /// <summary>
    /// Full path of the file to create. Missing directories are created; an existing file is never overwritten.
    /// </summary>
    /// <example>C:\output\data.txt</example>
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
    /// Prefix added to the error message when the task fails.
    /// </summary>
    /// <example>Converting JSON to fixed-width data failed.</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
