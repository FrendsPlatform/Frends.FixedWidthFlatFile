using System.Collections.Generic;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.


/// <summary>
/// Input parameters.
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
    public List<Dictionary<string, dynamic?>> FileContent { get; set; } = new List<Dictionary<string, dynamic?>>();
}

#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

