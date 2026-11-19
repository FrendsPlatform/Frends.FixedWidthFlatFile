namespace Frends.FixedWidthFlatFile.ConvertToObject.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Converted fixed width flat file data.
    /// </summary>
    /// <example>{[ Name, Foo ], [ Street, Bar ]}</example>
    public List<Dictionary<string, dynamic>> Data { get; set; } = [];

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
