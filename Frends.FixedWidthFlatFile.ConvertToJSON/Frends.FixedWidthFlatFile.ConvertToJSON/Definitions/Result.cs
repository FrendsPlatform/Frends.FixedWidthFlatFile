using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
