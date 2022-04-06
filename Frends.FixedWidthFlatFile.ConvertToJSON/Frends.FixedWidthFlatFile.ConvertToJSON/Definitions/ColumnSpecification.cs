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
    public class ColumnSpecification
    {
        /// <summary>
        /// Column name. If input data contains Header row and value is left empty, header value is used as name.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Name { get; set; }
        /// <summary>
        /// Column type. Type of input data.
        /// </summary>
        public ColumnType Type { get; set; }

        /// <summary>
        /// Exact format of DateTime value
        /// Example: yyyy-MM-ddTHH:mm:ss
        /// </summary>
        [UIHint(nameof(Type), "", ColumnType.DateTime)]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("yyyy-MM-ddTHH:mm:ss")]
        public string DateTimeFormat { get; set; }
        /// <summary>
        /// Column length. Determines length of column.
        /// </summary>
        public int Length { get; set; }
    }
}
