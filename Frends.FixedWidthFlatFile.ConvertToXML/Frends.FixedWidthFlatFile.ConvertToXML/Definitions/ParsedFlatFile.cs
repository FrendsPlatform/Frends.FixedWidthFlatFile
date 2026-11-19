using System.Collections.Generic;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Definitions;

/// <summary>
/// Fixed width flat file parsed into column names and typed row values.
/// </summary>
/// <param name="ColumnNames">Unique column names, in column order.</param>
/// <param name="Rows">Data rows. Each row holds one value per column, null for empty values.</param>
internal sealed record ParsedFlatFile(string[] ColumnNames, List<object[]> Rows);
