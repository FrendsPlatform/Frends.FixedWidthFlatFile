using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromXml.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromXml.Definitions;

/// <summary>
/// Column specification parameters.
/// </summary>
public class ColumnSpecification
{
    /// <summary>
    /// Name of the XML element holding the column value. If left empty, the element at the same position in the row is used
    /// and its name is used as the header value.
    /// </summary>
    /// <example>Example</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Column type.
    /// </summary>
    /// <example>ColumnType.String</example>
    [DefaultValue(ColumnType.String)]
    public ColumnType Type { get; set; } = ColumnType.String;

    /// <summary>
    /// Exact format used to write DateTime values. If empty, the default format of the culture is used.
    /// </summary>
    /// <example>yyyy-MM-ddTHH:mm:ss</example>
    [UIHint(nameof(Type), "", ColumnType.DateTime)]
    [RequiredIf(nameof(Type), "", ColumnType.DateTime)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Length of the column in characters. Values are padded with spaces to this length.
    /// </summary>
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "Column length must be greater than zero.")]
    public int Length { get; set; }
}
