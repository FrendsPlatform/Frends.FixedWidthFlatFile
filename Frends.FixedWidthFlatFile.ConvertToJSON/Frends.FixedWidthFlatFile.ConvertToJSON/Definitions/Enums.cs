namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// HeaderRowType values.
/// </summary>
public enum HeaderRowType
{
    /// <summary>
    /// Represents no value.
    /// </summary>
    None,

    /// <summary>
    /// Represents fixed-width formatting.
    /// </summary>
    FixedWidth,

    /// <summary>
    /// Represents content formatted as delimited values.
    /// </summary>
    Delimited,
}

/// <summary>
/// ColumnType values.
/// </summary>
public enum ColumnType
{
#pragma warning disable SA1602 // self explanatory
    String,
    Int,
    Long,
    Decimal,
    Double,
    Boolean,
    DateTime,
    Char,
#pragma warning restore SA1602 // self explanatory
}
