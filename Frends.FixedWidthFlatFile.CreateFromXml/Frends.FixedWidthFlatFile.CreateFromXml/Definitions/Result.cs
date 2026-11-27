namespace Frends.FixedWidthFlatFile.CreateFromXml.Definitions;

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
    /// The created fixed width flat file content. Null when the destination type is File.
    /// </summary>
    /// <example>
    /// Name    Street    StartDate
    /// Veijo   FrendsStr 20180527
    /// </example>
    public string Data { get; set; }

    /// <summary>
    /// Full path of the created flat file. Null when the destination type is String.
    /// </summary>
    /// <example>C:\output\data.txt</example>
    public string FilePath { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
