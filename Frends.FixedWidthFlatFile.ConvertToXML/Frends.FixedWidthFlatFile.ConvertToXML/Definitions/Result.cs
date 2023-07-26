namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Result class.
/// </summary>
public class Result
{
    /// <summary>
    /// Column name. If input data contains Header row and value is left empty, header value is used as name.
    /// </summary>
    /// <example>
    /// &lt;?xml version="1.0" encoding="utf-8"?&gt;
    /// &lt;Root&gt;
    ///  &lt;Rows&gt;
    ///    &lt;Row&gt;
    ///      &lt;Name&gt;Veijo&lt;/Name&gt;
    ///      &lt;Street&gt;FrendsStr&lt;/Street&gt;
    ///      &lt;StartDate&gt;2018-05-27T00:00:00&lt;/StartDate&gt;
    ///    &lt;/Row&gt;
    ///    &lt;Row&gt;
    ///      &lt;Name&gt;Hodor&lt;/Name&gt;
    ///      &lt;Street&gt;HodorsStr&lt;/Street&gt;
    ///      &lt;StartDate&gt;2018-01-01T00:00:00&lt;/StartDate&gt;
    ///    &lt;/Row&gt;
    ///  &lt;/Rows&gt;
    /// &lt;/Root&gt;
    /// </example>
    public string Data { get; private set; }

    internal Result(string data)
    {
        Data = data;
    }
}