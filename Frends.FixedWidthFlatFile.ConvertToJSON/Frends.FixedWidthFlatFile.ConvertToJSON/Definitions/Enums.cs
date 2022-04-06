using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Definitions
{
    public class Enums
    {
        /// <summary>
        /// Header row type. Can be "none", "fixedWidth", "delimited".
        /// </summary>
        public enum HeaderRowType { None, FixedWidth, Delimited }
        /// <summary>
        /// Column type. Can be "string", "int", "decimal", "double", "boolean", "dateTime", "char".
        /// </summary>
        public enum ColumnType { String, Int, Long, Decimal, Double, Boolean, DateTime, Char }
    }
}
