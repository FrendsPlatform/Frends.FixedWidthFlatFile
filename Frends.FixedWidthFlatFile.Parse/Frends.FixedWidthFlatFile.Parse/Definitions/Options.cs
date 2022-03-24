using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

public class Options
{
    /// <summary>
    /// Skip data rows?
    /// </summary>
    [DefaultValue(false)]
    public bool SkipRows { get; set; }

    /// <summary>
    /// Count of data rows to skip from top
    /// </summary>
    [UIHint(nameof(SkipRows), "", true)]
    [DefaultValue(0)]
    public int SkipRowsFromTop { get; set; }

    /// <summary>
    /// Count of data rows to skip from bottom
    /// </summary>
    [UIHint(nameof(SkipRows), "", true)]
    [DefaultValue(0)]
    public int SkipRowsFromBottom { get; set; }
}
