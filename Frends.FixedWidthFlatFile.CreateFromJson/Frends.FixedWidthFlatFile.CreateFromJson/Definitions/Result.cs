namespace Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

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
    /// Fixed-width flat file content. Records are separated by the platform newline.
    /// Null when the destination type is File.
    /// </summary>
    /// <example>Veijo   20180527</example>
    public string FlatFileContent { get; set; }

    /// <summary>
    /// Full path of the created file. Null when the destination type is String.
    /// </summary>
    /// <example>C:\output\data.txt</example>
    public string FlatFilePath { get; set; }

    /// <summary>
    /// Error details when the task fails; null otherwise.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; set; }
}
