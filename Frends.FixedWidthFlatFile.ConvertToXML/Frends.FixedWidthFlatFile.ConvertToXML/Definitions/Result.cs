using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions
{
    public class Result
    {
        /// <summary>
        /// Column name. If input data contains Header row and value is left empty, header value is used as name.
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Result(string data) { 
            this.Data = data;
        }
    }
}
