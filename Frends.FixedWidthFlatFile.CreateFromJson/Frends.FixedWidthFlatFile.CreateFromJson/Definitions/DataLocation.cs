namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Defines where data is provided from or written to.
/// </summary>
public enum DataLocation
{
    /// <summary>Data is handled as a string.</summary>
    String,

    /// <summary>Data is handled as a file.</summary>
    File,
}
