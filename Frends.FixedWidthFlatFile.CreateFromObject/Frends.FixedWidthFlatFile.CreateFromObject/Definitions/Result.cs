namespace Frends.FixedWidthFlatFile.CreateFromObject.Definitions;

/// <summary>Result of the fixed-width formatting task.</summary>
public class Result
{
    /// <summary>
    /// Indicates whether the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Generated fixed-width text, including the optional header row. Null when written to a file.
    /// </summary>
    /// <example>Name    StartDate
    /// Veijo   20180527</example>
    public string FlatFileContent { get; set; }

    /// <summary>
    /// Full path of the created file. Null when content is returned as a string.
    /// </summary>
    /// <example>C:\output\data.txt</example>
    public string FilePath { get; set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
