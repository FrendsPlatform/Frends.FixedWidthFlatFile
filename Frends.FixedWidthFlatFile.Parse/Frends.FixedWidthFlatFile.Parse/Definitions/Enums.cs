namespace Frends.FixedWidthFlatFile.Parse.Definitions;

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
#pragma warning disable CS1591 // self explanatory
#pragma warning disable SA1602 // Enumeration items should be documented
    String,

    Int,
    Long,
    Decimal,
    Double,
    Boolean,
    DateTime,
    Char,
#pragma warning restore CS1591 // self explanatory
#pragma warning restore SA1602 // Enumeration items should be documented
}