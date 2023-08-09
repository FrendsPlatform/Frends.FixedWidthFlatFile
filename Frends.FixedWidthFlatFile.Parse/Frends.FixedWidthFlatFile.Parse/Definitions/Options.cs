using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Options parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Enables skipping of data rows.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool SkipRows { get; set; }

    /// <summary>
    /// Count of data rows to skip from top.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [DefaultValue(0)]
    public int SkipRowsFromTop { get; set; }

    /// <summary>
    /// Count of data rows to skip from bottom.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(SkipRows), "", true)]
    [DefaultValue(0)]
    public int SkipRowsFromBottom { get; set; }
}