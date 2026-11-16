using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromJson.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Whether the input JSON is provided as a string or read from a file.
    /// </summary>
    /// <example>String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation SourceType { get; set; } = DataLocation.String;

    /// <summary>
    /// JSON array of objects to write. Each object property is matched to a column by name.
    /// Missing and null properties are written as spaces; additional properties are ignored.
    /// </summary>
    /// <example>[{"Name":"Veijo","StartDate":"2018-05-27T00:00:00"}]</example>
    [UIHint(nameof(SourceType), "", DataLocation.String)]
    [RequiredIf(nameof(SourceType), DataLocation.String)]
    [DisplayFormat(DataFormatString = "Json")]
    public string Json { get; set; } = string.Empty;

    /// <summary>
    /// Path to a file containing the JSON array of objects.
    /// </summary>
    /// <example>C:\input\data.json</example>
    [UIHint(nameof(SourceType), "", DataLocation.File)]
    [RequiredIf(nameof(SourceType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string JsonFilePath { get; set; } = string.Empty;

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
