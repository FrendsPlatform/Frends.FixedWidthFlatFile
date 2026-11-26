using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromObject.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>
/// Defines how a column is written.
/// </summary>
public class ColumnSpecification
{
    /// <summary>
    /// Column name used to find the value in each data row and, when enabled, written in the header.
    /// </summary>
    /// <example>Name</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Value type. DateTime values are formatted using DateTimeFormat; other values use invariant culture.
    /// </summary>
    /// <example>ColumnType.String</example>
    [DefaultValue(ColumnType.String)]
    public ColumnType Type { get; set; } = ColumnType.String;

    /// <summary>
    /// Exact format used for DateTime values.
    /// </summary>
    /// <example>yyyyMMdd</example>
    [UIHint(nameof(Type), "", ColumnType.DateTime)]
    [RequiredIf(nameof(Type), ColumnType.DateTime)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Width of the column in characters. Values are left-aligned and padded with spaces; values wider than the column fail.
    /// </summary>
    /// <example>8</example>
    [Range(1, int.MaxValue)]
    public int Length { get; set; }
}
