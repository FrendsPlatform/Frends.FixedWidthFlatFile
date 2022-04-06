using System.ComponentModel.DataAnnotations;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions
{
    public class Result
    {
        /// <summary>
        /// Column name. If input data contains Header row and value is left empty, header value is used as name.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Data { get; set; }
    }
}
