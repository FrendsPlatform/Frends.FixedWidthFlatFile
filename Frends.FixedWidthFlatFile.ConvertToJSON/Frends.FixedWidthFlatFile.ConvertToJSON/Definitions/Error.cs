using System;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// Error information.
/// </summary>
public class Error
{
    /// <summary>
    /// Error message.
    /// </summary>
    /// <example>JSON parse failed.</example>
    public string Message { get; set; }

    /// <summary>
    /// Exception that caused the error, if any.
    /// </summary>
    /// <example>object { Exception AdditionalInfo }</example>
    public Exception AdditionalInfo { get; set; }
}
