namespace Frends.FixedWidthFlatFile.Parse.Definitions;

/// <summary>
/// Result of fixed width flat file parse Task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error? Error { get; private set; }

    /// <summary>
    /// Parsed fixed flat file data.
    /// </summary>
    /// <example>{[ Name, Foo ], [ Street, Bar ]}</example>
    public List<Dictionary<string, object?>> Data { get; private set; }

    internal Result(bool success, List<Dictionary<string, object?>> data, Error? error = null)
    {
        Success = success;
        Data = data;
        Error = error;
    }
}
