namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// Result class.
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
    public Error Error { get; private set; }

    /// <summary>
    /// Converted json data.
    /// </summary>
    /// <example>
    /// [
    ///     {
    ///         "Name": "Veijo",
    ///         "Street": "FrendsStr",
    ///         "StartDate": "2018-05-27T00:00:00"
    ///     },
    ///     {
    ///         "Name": "Hodor",
    ///         "Street": "HodorsStr",
    ///         "StartDate": "2018-01-01T00:00:00"
    ///     }
    /// ]
    /// </example>
    public string Data { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Whether the operation completed successfully.</param>
    /// <param name="data">Converted json data. Null on failure.</param>
    /// <param name="error">Error details. Null when success is true.</param>
    public Result(bool success, string data = null, Error error = null)
    {
        Success = success;
        Data = data;
        Error = error;
    }
}
