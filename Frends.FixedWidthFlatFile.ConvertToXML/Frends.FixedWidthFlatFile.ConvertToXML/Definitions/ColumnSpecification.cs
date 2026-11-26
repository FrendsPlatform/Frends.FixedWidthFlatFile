using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Column specification parameters.
/// </summary>
public class ColumnSpecification
{
    /// <summary>
    /// Column name, used as the XML element name. If the flat file contains a header row and the value is left empty, the header value is used as the name.
    /// If there is no name at all, the name is generated as Field_{column number}.
    /// </summary>
    /// <example>Example</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; } = string.Empty;

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
    /// Length of the column in characters.
    /// </summary>
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "Column length must be greater than zero.")]
    public int Length { get; set; }
}
