using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// JSON array of objects to write. Each object property is matched to a column by name.
    /// Missing and null properties are written as spaces; additional properties are ignored.
    /// </summary>
    /// <example>[{"Name":"Veijo","StartDate":"2018-05-27T00:00:00"}]</example>
    [DisplayFormat(DataFormatString = "Json")]
    [Required]
    public string Json { get; set; } = string.Empty;

    /// <summary>
    /// Column definitions in the order they appear in each fixed-width record.
    /// </summary>
    /// <example>[{"Name":"Name","Type":"String","Length":8},{"Name":"StartDate","Type":"DateTime","DateTimeFormat":"yyyyMMdd","Length":8}]</example>
    [MinLength(1)]
    [Required]
    public ColumnSpecification[] ColumnSpecifications { get; set; } = [];

    /// <summary>
    /// Whether to include a fixed-width header row containing the column names.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool IncludeHeaderRow { get; set; }

    /// <summary>
    /// Culture used to format numeric and date values. An empty value uses invariant culture.
    /// </summary>
    /// <example>fi-FI</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string Culture { get; set; } = string.Empty;
}
