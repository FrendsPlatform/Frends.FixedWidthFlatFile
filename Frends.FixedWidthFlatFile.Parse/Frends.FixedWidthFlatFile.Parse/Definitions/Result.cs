namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Result of fixed width flat file parse task.
/// </summary>
public class Result
{
    /// <summary>
    /// Parsed fixed flat file data.
    /// </summary>
    public List<Dictionary<string, dynamic?>> Data { get; internal set; } = new List<Dictionary<string, dynamic?>>();
}
