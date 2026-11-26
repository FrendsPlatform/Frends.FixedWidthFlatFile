namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>
/// Header row output format.
/// </summary>
public enum HeaderRowType
{
    /// <summary>
    /// Do not write a header row.
    /// </summary>
    None,

    /// <summary>
    /// Write column names using the configured fixed widths.
    /// </summary>
    FixedWidth,

    /// <summary>
    /// Write column names separated by HeaderDelimiter.
    /// </summary>
    Delimited,
}

/// <summary>
/// Supported column value types.
/// </summary>
public enum ColumnType
{
#pragma warning disable SA1602 // The enum member names are self-documenting.
    String,
    Int,
    Long,
    Decimal,
    Double,
    Boolean,
    DateTime,
    Char,
#pragma warning restore SA1602 // The enum member names are self-documenting.
}
