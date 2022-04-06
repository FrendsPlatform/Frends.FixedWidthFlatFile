using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions
{
    public class Options
    {
        /// <summary>
        /// Skip data rows?
        /// </summary>
        [DefaultValue(false)]
        public bool SkipRows { get; set; }

        /// <summary>
        /// Count of data rows to skip from top
        /// </summary>
        [UIHint(nameof(SkipRows), "", true)]
        [DefaultValue(0)]
        public int SkipRowsFromTop { get; set; }

        /// <summary>
        /// Count of data rows to skip from bottom
        /// </summary>
        [UIHint(nameof(SkipRows), "", true)]
        [DefaultValue(0)]
        public int SkipRowsFromBottom { get; set; }

        /// <summary>
        /// Specify the culture info to be used when parsing result to JTOKEN. If this is left empty InvariantCulture will be used. List of cultures: https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx Use the Language Culture Name.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string CultureInfo { get; set; }
    }
}
