namespace Frends.FixedWidthFlatFile.CreateFromXml.Definitions;

/// <summary>
/// HeaderRowType values.
/// </summary>
public enum HeaderRowType
{
    /// <summary>
    /// Flat file is created without a header row.
    /// </summary>
    None,

    /// <summary>
    /// Header row is written using column specification widths.
    /// </summary>
    FixedWidth,

    /// <summary>
    /// Header row is written using delimiter char.
    /// </summary>
    Delimited,
}

/// <summary>
/// ColumnType values.
/// </summary>
public enum ColumnType
{
#pragma warning disable SA1602 // self-explanatory
    String,
    Int,
    Long,
    Decimal,
    Double,
    Boolean,
    DateTime,
    Char,
#pragma warning restore SA1602 // self-explanatory
}

/// <summary>
/// Defines where data is provided from or written to.
/// </summary>
public enum DataLocation
{
#pragma warning disable SA1602 // self-explanatory
    String,
    File,
#pragma warning restore SA1602 // self-explanatory
}
