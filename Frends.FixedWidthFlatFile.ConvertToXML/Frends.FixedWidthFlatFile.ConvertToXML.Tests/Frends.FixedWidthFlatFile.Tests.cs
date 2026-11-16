using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Tests;

[TestFixture]
public class Tests
{
    private const string FixedWidthHeaderFile =
        "Name    Street    StartDate\n" +
        "Veijo   FrendsStr 20180527 \n" +
        "Hodor   HodorsStr 20180101 ";

    private static ColumnSpecification[] NamedColumns() => new[]
    {
        new ColumnSpecification { Name = "Name", Length = 8, Type = ColumnType.String },
        new ColumnSpecification { Name = "Street", Length = 10, Type = ColumnType.String },
        new ColumnSpecification { Name = "StartDate", Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd" },
    };

    private static ColumnSpecification[] UnnamedColumns() => new[]
    {
        new ColumnSpecification { Length = 8, Type = ColumnType.String },
        new ColumnSpecification { Length = 10, Type = ColumnType.String },
        new ColumnSpecification { Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd" },
    };

    private static List<Dictionary<string, string>> ReadRows(string xml)
    {
        var document = XDocument.Parse(xml);
        Assert.That(document.Root.Name.LocalName, Is.EqualTo("Root"));

        return document.Root.Element("Rows")
            .Elements("Row")
            .Select(row => row.Elements().ToDictionary(e => e.Name.LocalName, e => e.Value))
            .ToList();
    }

    [Test]
    public void ConvertToXML_FixedWidthHeader_ReturnsXmlWithRowsAndHeaderNames()
    {
        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = UnnamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(result.Data, Does.StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));

        var rows = ReadRows(result.Data);
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0], Is.EqualTo(new Dictionary<string, string>
        {
            ["Name"] = "Veijo",
            ["Street"] = "FrendsStr",
            ["StartDate"] = "2018-05-27T00:00:00",
        }));
        Assert.That(rows[1]["Name"], Is.EqualTo("Hodor"));
        Assert.That(rows[1]["StartDate"], Is.EqualTo("2018-01-01T00:00:00"));
    }

    [Test]
    public void ConvertToXML_DelimitedHeader_UsesHeaderValuesAsNames()
    {
        var input = new Input
        {
            FlatFileContent = "Name;Street;StartDate\nVeijo   FrendsStr 20180527 \nHodor   HodorsStr 20180101 ",
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = ";",
            ColumnSpecifications = UnnamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var rows = ReadRows(result.Data);
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0].Keys, Is.EqualTo(new[] { "Name", "Street", "StartDate" }));
        Assert.That(rows[0]["Street"], Is.EqualTo("FrendsStr"));
    }

    [Test]
    public void ConvertToXML_NoHeader_UsesColumnSpecificationNames()
    {
        var input = new Input
        {
            FlatFileContent = "Veijo   FrendsStr 20180527 \nHodor   HodorsStr 20180101 ",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = NamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var rows = ReadRows(result.Data);
        Assert.That(rows, Has.Count.EqualTo(2));
        Assert.That(rows[0].Keys, Is.EqualTo(new[] { "Name", "Street", "StartDate" }));
        Assert.That(rows[1]["Name"], Is.EqualTo("Hodor"));
    }

    [Test]
    public void ConvertToXML_SpecificationNameWinsOverHeaderValue()
    {
        var columns = UnnamedColumns();
        columns[0].Name = "Person";

        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = columns,
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(ReadRows(result.Data)[0].Keys, Is.EqualTo(new[] { "Person", "Street", "StartDate" }));
    }

    [Test]
    public void ConvertToXML_GeneratesFieldNamesForUnnamedColumns()
    {
        var input = new Input
        {
            FlatFileContent = "Veijo   FrendsStr 20180527 ",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = UnnamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var row = ReadRows(result.Data).Single();
        Assert.That(row.Keys, Is.EqualTo(new[] { "Field_1", "Field_2", "Field_3" }));
        Assert.That(row["Field_2"], Is.EqualTo("FrendsStr"));
    }

    [Test]
    public void ConvertToXML_RenamesDuplicateColumnNames()
    {
        var input = new Input
        {
            FlatFileContent = "aabbcc",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[]
            {
                new ColumnSpecification { Name = "Col", Length = 2 },
                new ColumnSpecification { Name = "Col", Length = 2 },
                new ColumnSpecification { Name = "Col", Length = 2 },
            },
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var row = ReadRows(result.Data).Single();
        Assert.That(row.Keys, Is.EqualTo(new[] { "Col", "Col_1", "Col_2" }));
        Assert.That(row.Values, Is.EqualTo(new[] { "aa", "bb", "cc" }));
    }

    [Test]
    public void ConvertToXML_EmptyValuesAreWrittenAsEmptyElements()
    {
        var input = new Input
        {
            FlatFileContent = "Veijo   " + new string(' ', 10) + "20180527 ",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = NamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var row = ReadRows(result.Data).Single();
        Assert.That(row["Street"], Is.Empty);
        Assert.That(row["Name"], Is.EqualTo("Veijo"));
    }

    [Test]
    public void ConvertToXML_EmptyValueInUnnamedColumnUsesGeneratedName()
    {
        var input = new Input
        {
            FlatFileContent = "Veijo   " + new string(' ', 10) + "20180527 ",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = UnnamedColumns(),
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var row = ReadRows(result.Data).Single();
        Assert.That(row.Keys, Is.EqualTo(new[] { "Field_1", "Field_2", "Field_3" }));
        Assert.That(row["Field_2"], Is.Empty);
    }

    [Test]
    [SetCulture("pl-PL")]
    public void ConvertToXML_FormatsTypedValuesIndependentOfCurrentCulture()
    {
        var input = new Input
        {
            FlatFileContent = "truet1,5123456789020180424912,2512.5",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[]
            {
                new ColumnSpecification { Type = ColumnType.Boolean, Length = 4, Name = "Boolean" },
                new ColumnSpecification { Type = ColumnType.Char, Length = 1, Name = "Char" },
                new ColumnSpecification { Type = ColumnType.Double, Length = 3, Name = "Double" },
                new ColumnSpecification { Type = ColumnType.Long, Length = 10, Name = "Long" },
                new ColumnSpecification { Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 8, Name = "DateTime" },
                new ColumnSpecification { Type = ColumnType.Int, Length = 1, Name = "Int" },
                new ColumnSpecification { Type = ColumnType.Decimal, Length = 5, Name = "Decimal" },
                new ColumnSpecification { Type = ColumnType.Double, Length = 4, Name = "InvariantDouble" },
            },
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        var row = ReadRows(result.Data).Single();
        Assert.That(row["Boolean"], Is.EqualTo("True"));
        Assert.That(row["Char"], Is.EqualTo("t"));
        Assert.That(row["Double"], Is.EqualTo("1.5"));
        Assert.That(row["Long"], Is.EqualTo("1234567890"));
        Assert.That(row["DateTime"], Is.EqualTo("2018-04-24T00:00:00"));
        Assert.That(row["Int"], Is.EqualTo("9"));
        Assert.That(row["Decimal"], Is.EqualTo("12.25"));
        Assert.That(row["InvariantDouble"], Is.EqualTo("12.5"));
    }

    [Test]
    public void ConvertToXML_SkipRows_SkipsFromTopAndBottom()
    {
        var input = new Input
        {
            FlatFileContent = "a1\na2\na3\na4\na5",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[] { new ColumnSpecification { Name = "Value", Length = 2 } },
        };
        var options = new Options { SkipRows = true, SkipRowsFromTop = 1, SkipRowsFromBottom = 2 };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(ReadRows(result.Data).Select(r => r["Value"]), Is.EqualTo(new[] { "a2", "a3" }));
    }

    [Test]
    public void ConvertToXML_SkipRowsDisabled_IgnoresSkipCounts()
    {
        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = UnnamedColumns(),
        };
        var options = new Options { SkipRows = false, SkipRowsFromTop = 5, SkipRowsFromBottom = 5 };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(ReadRows(result.Data), Has.Count.EqualTo(2));
    }

    [Test]
    public void ConvertToXML_SkippingMoreRowsThanExist_ReturnsEmptyRows()
    {
        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = UnnamedColumns(),
        };
        var options = new Options { SkipRows = true, SkipRowsFromTop = 1, SkipRowsFromBottom = 5 };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(ReadRows(result.Data), Is.Empty);
    }

    [Test]
    public void ConvertToXML_IgnoresEmptyLines()
    {
        var input = new Input
        {
            FlatFileContent = "\naa\n\n   \nbb\n",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[] { new ColumnSpecification { Name = "Value", Length = 2 } },
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(ReadRows(result.Data).Select(r => r["Value"]), Is.EqualTo(new[] { "aa", "bb" }));
    }

    [Test]
    public void ConvertToXML_DoesNotModifyInputColumnSpecifications()
    {
        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = UnnamedColumns(),
        };

        FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(input.ColumnSpecifications.Select(c => c.Name), Is.All.Empty);
    }

    [Test]
    public void ConvertToXML_HeaderWithMoreColumnsThanSpecifications_IgnoresExtraHeaders()
    {
        var input = new Input
        {
            FlatFileContent = "A;B;C\naabb",
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = ";",
            ColumnSpecifications = new[]
            {
                new ColumnSpecification { Length = 2 },
                new ColumnSpecification { Length = 2 },
            },
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(ReadRows(result.Data).Single().Keys, Is.EqualTo(new[] { "A", "B" }));
    }

    [Test]
    public void ConvertToXML_RowNotMatchingColumnSpecifications_ThrowsInvalidDataException()
    {
        var input = new Input
        {
            FlatFileContent = "Veijo",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = NamedColumns(),
        };

        var ex = Assert.Throws<InvalidDataException>((Action)(() =>
            FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None)));

        Assert.That(ex.Message, Is.EqualTo("Data row did not match column specifications."));
    }

    [Test]
    public void ConvertToXML_InvalidValueForColumnType_ReturnsFailedResultWhenNotThrowing()
    {
        var input = new Input
        {
            FlatFileContent = "abc",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[] { new ColumnSpecification { Name = "Number", Length = 3, Type = ColumnType.Int } },
        };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<FormatException>());
    }

    [Test]
    public void ConvertToXML_EmptyFlatFileContent_ReturnsErrorMessage()
    {
        var input = new Input { FlatFileContent = "  \n ", ColumnSpecifications = NamedColumns() };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Is.EqualTo("FlatFileContent not given. Cannot be empty."));
    }

    [Test]
    public void ConvertToXML_NoColumnSpecifications_ReturnsErrorMessage()
    {
        var input = new Input { FlatFileContent = "Veijo", HeaderRow = HeaderRowType.None };

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options { ThrowErrorOnFailure = false }, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Is.EqualTo("ColumnSpecifications not given. Cannot be empty."));
    }

    [Test]
    public void ConvertToXML_CanceledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var input = new Input
        {
            FlatFileContent = FixedWidthHeaderFile,
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications = UnnamedColumns(),
        };

        Assert.Throws<OperationCanceledException>((Action)(() =>
            FixedWidthFlatFile.ConvertToXML(input, new Options { ThrowErrorOnFailure = false }, cts.Token)));
    }
}
