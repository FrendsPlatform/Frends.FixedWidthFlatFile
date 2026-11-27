using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FixedWidthFlatFile.CreateFromXml.Attributes;

namespace Frends.FixedWidthFlatFile.CreateFromXml.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Whether the XML is provided as a string or read from a file.
    /// </summary>
    /// <example>String</example>
    [DefaultValue(DataLocation.String)]
    public DataLocation SourceType { get; set; } = DataLocation.String;

    /// <summary>
    /// XML content in the format Root/Rows/Row/{column}.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;Root&gt;&lt;Rows&gt;&lt;Row&gt;&lt;Name&gt;Veijo&lt;/Name&gt;&lt;/Row&gt;&lt;/Rows&gt;&lt;/Root&gt;
    /// </code> </example>
    [UIHint(nameof(SourceType), "", DataLocation.String)]
    [RequiredIf(nameof(SourceType), DataLocation.String)]
    [DisplayFormat(DataFormatString = "Text")]
    public string XmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Path to a file containing the XML.
    /// </summary>
    /// <example>C:\input\data.xml</example>
    [UIHint(nameof(SourceType), "", DataLocation.File)]
    [RequiredIf(nameof(SourceType), DataLocation.File)]
    [DisplayFormat(DataFormatString = "Text")]
    public string XmlFilePath { get; set; } = string.Empty;
}
