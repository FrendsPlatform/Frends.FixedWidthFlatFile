using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>
/// Data and fixed-width layout parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Rows to write. Each row must contain a value for every configured column; null values are written as spaces.
    /// </summary>
    /// <example>[{ "Name": "Veijo", "StartDate": "2018-05-27T00:00:00" }]</example>
    [Required]
    public List<Dictionary<string, object>> Data { get; set; } = [];
}
