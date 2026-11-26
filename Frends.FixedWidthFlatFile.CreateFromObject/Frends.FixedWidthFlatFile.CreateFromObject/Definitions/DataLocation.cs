namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>
/// Defines whether output is returned as a string or written to a file.
/// </summary>
public enum DataLocation
{
    /// <summary>
    /// Return the generated content as a string.
    /// </summary>
    String,

    /// <summary>
    /// Write the generated content to a file.
    /// </summary>
    File,
}
