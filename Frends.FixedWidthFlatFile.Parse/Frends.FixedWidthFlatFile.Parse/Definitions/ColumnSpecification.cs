using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

public class ColumnSpecification
{
    /// <summary>
    /// Column name. If input data contains Header row and value is left empty, header value is used as name.
    /// </summary>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }
    public ColumnType Type { get; set; }

    /// <summary>
    /// Exact format of DateTime value
    /// Example: yyyy-MM-ddTHH:mm:ss
    /// </summary>
    [UIHint(nameof(Type), "", ColumnType.DateTime)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
    public string DateTimeFormat { get; set; }
    public int Length { get; set; }
}