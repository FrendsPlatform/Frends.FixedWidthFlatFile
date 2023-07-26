using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// Result class.
/// </summary>
public class Result
{
    /// <summary>
    /// Column name. If input data contains Header row and value is left empty, header value is used as name.
    /// </summary>
    /// <example>
	/// [
	///		{
	/// 		"Name": "Veijo",
	/// 		"Street": "FrendsStr",
	/// 		"StartDate": "2018-05-27T00:00:00"
	///		},
	///		{
	/// 		"Name": "Hodor",
	/// 		"Street": "HodorsStr",
	/// 		"StartDate": "2018-01-01T00:00:00"
	///		}
	/// ]
	/// </example>
    public string Data { get; private set; }

    internal Result(string data) 
    { 
        Data = data;
    }
}

