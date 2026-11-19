namespace Frends.FixedWidthFlatFile.ConvertToObject.Definitions;

#pragma warning disable SA1602 // self explanatory
public enum HeaderRowType
{
    None,
    FixedWidth,
    Delimited,
}

public enum ColumnType
{
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
