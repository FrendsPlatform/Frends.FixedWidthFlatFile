using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Frends.FixedWidthFlatFile.ConvertToJSON.Definitions.Enums;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions
{
    public class Input
    {
        /// <summary>
        /// Fixed width flat file content
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public List<Dictionary<string, dynamic?>> FileContent { get; set; } = new List<Dictionary<string, dynamic?>>();
        
        /// <summary>
        /// Culture format json will be build with
        /// </summary>
        [DisplayFormat(DataFormatString = "text")]
        public string culture = null;
    }
}
