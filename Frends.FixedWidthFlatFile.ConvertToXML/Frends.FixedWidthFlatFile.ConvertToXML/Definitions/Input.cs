using System.Collections.Generic;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions
{
    public class Input
    {
        /// <summary>
        /// Fixed width flat file content
        /// </summary>
        public List<Dictionary<string, dynamic?>> FileContent { get; set; } = new List<Dictionary<string, dynamic?>>();
    }
}
