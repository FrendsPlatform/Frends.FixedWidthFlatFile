using System;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Error that occurred during the Task.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Data row did not match column specifications.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional information about the error.
    /// </summary>
    /// <example>object { Exception AdditionalInfo }</example>
    public Exception? AdditionalInfo { get; set; }
}
