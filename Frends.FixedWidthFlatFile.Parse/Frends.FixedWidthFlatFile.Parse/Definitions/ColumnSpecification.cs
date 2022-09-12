using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Column specification parameters.
/// </summary>
public class ColumnSpecification
{
    /// <summary>
    /// Column name. If input data contains Header row and value is left empty, header value is used as name.
    /// </summary>
    /// <example>Example</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Column type.
    /// </summary>
    /// <example>ColumnType.String</example>
    [DefaultValue(ColumnType.String)]
    public ColumnType Type { get; set; }

    /// <summary>
    /// Exact format of DateTime value.
    /// </summary>
    /// <example>yyyy-MM-ddTHH:mm:ss</example>
    [UIHint(nameof(Type), "", ColumnType.DateTime)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Lenght.
    /// </summary>
    /// <example>5</example>
    public int Length { get; set; }
}