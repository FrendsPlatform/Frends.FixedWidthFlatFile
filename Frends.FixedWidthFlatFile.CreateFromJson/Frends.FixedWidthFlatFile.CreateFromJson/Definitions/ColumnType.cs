namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Supported column value types.
/// </summary>
public enum ColumnType
{
    /// <summary>Text value.</summary>
    String,

    /// <summary>32-bit integer.</summary>
    Int,

    /// <summary>64-bit integer.</summary>
    Long,

    /// <summary>Decimal number.</summary>
    Decimal,

    /// <summary>Floating-point number.</summary>
    Double,

    /// <summary>Boolean value.</summary>
    Boolean,

    /// <summary>Date and time value.</summary>
    DateTime,

    /// <summary>Single character.</summary>
    Char,
}
