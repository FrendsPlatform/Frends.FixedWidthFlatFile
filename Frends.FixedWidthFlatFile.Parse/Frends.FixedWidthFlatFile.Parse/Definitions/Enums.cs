namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// HeaderRowType values.
/// </summary>
public enum HeaderRowType 
{
#pragma warning disable CS1591 // self explanatory
    None,
    FixedWidth, 
    Delimited
#pragma warning restore CS1591 // self explanatory
}

/// <summary>
/// ColumnType values.
/// </summary>
public enum ColumnType 
{
#pragma warning disable CS1591 // self explanatory
    String,
    Int, 
    Long, 
    Decimal, 
    Double, 
    Boolean, 
    DateTime, 
    Char
#pragma warning restore CS1591 // self explanatory
}