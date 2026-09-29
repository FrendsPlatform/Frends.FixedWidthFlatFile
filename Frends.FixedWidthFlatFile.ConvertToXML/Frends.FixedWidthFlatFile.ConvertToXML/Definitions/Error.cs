using System;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Error class.
/// </summary>
public class Error
{
    /// <summary>
    /// Error message.
    /// </summary>
    /// <example>FileContent not given. Cannot be empty.</example>
    public string Message { get; set; }

    /// <summary>
    /// Additional information about the error, e.g. the original exception.
    /// </summary>
    /// <example>System.ArgumentNullException: FileContent not given. Cannot be empty.</example>
    public Exception AdditionalInfo { get; set; }
}
