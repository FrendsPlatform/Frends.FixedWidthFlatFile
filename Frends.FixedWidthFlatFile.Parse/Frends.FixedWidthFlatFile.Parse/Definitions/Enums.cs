using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frends.FixedWidthFlatFile.Parse.Definitions;

public enum HeaderRowType { None, FixedWidth, Delimited }

public enum ColumnType { String, Int, Long, Decimal, Double, Boolean, DateTime, Char }