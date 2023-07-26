using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;

/// <summary>
/// Input parameters for the Task.
/// </summary>
public class Input
{
    /// <summary>
    /// Fixed width flat file content
    /// </summary>
    /// <example>
    /// [
    ///	    {
    ///		    "Name": "Veijo",
    ///		    "Street": "FrendsStr",
    ///		    "StartDate": "2018-05-27T00:00:00"
    ///	    },
    ///	    {
    ///		    "Name": "Hodor",
    ///		    "Street": "HodorsStr",
    ///		    "StartDate": "2018-01-01T00:00:00"
    ///	    }
    /// ]
    /// </example>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    public List<Dictionary<string, dynamic?>> FileContent { get; set; } = new List<Dictionary<string, dynamic?>>();
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

    /// <summary>
    /// Culture format json will be build with
    /// </summary>
    /// <example>fi-FI</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string culture { get; set; } = string.Empty;
}
