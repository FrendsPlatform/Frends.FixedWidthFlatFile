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
        public string FlatFileContent { get; set; }

        /// <summary>
        /// None: Flat file does not contain header row
        /// FixedWidth: Header row is parsed using column specification
        /// Delimited: Header row is parsed using delimiter char
        /// </summary>
        [DefaultValue(HeaderRowType.FixedWidth)]
        public HeaderRowType HeaderRow { get; set; }

        /// <summary>
        /// If header row uses delimiter set it here
        /// </summary>
        [UIHint(nameof(HeaderRow), "", HeaderRowType.Delimited)]
        [DisplayFormat(DataFormatString = "Text")]
        public string HeaderDelimiter { get; set; }

        /// <summary>
        /// Column specification. Array of class ColumnSpecification.
        /// </summary>
        public ColumnSpecification[] ColumnSpecifications { get; set; }
    }
}
