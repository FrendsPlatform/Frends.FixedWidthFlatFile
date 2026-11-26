using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromJson.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Defines the name, type, and width of one output column.
/// </summary>
public class ColumnSpecification
{
    /// <summary>
    /// Property name in each JSON object. Also used as the header text when a header row is requested.
    /// </summary>
    /// <example>Name</example>
    [DisplayFormat(DataFormatString = "Text")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data type used to format and validate the JSON value.
    /// </summary>
    /// <example>ColumnType.String</example>
    [DefaultValue(ColumnType.String)]
    public ColumnType Type { get; set; }

    /// <summary>
    /// Output format used for date values (required for DateTime columns). Date strings in JSON are parsed using invariant culture; the output is formatted using the selected culture.
    /// </summary>
    /// <example>yyyyMMdd</example>
    [UIHint(nameof(Type), "", ColumnType.DateTime)]
    [RequiredIf(nameof(Type), ColumnType.DateTime)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
    public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";

    /// <summary>
    /// Exact width of the output field in characters. Values longer than this width cause an error.
    /// </summary>
    /// <example>8</example>
    [Range(1, int.MaxValue)]
    public int Length { get; set; }
}
