using System;
using System.Linq;
using Frends.FixedWidthFlatFile.Parse.Definitions;
using NUnit.Framework;
namespace Frends.FixedWidthFlatFile.Parse.Tests;

[TestFixture]
public class ExtensionTests
{
    [Test]
    public void SplitToList_WithDelimiter_Test()
    {
        var input = "h1;h2;h3;h4,h5";
        var result = FixedWidthFlatFile.SplitToList(input, ';');

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("h4,h5", result.Last());
    }

    [Test]
    public void SplitToList_FixedWidth_Test()
    {
        var input = "hodor22tenchars10";
        var columnSpecification = new ColumnSpecification[3];
        columnSpecification[0] = new ColumnSpecification { Name = "h1", Length = 5, Type = ColumnType.String };
        columnSpecification[1] = new ColumnSpecification { Name = "h2", Length = 2, Type = ColumnType.Int };
        columnSpecification[2] = new ColumnSpecification { Name = "h3", Length = 10, Type = ColumnType.String };

        var result = FixedWidthFlatFile.SplitToList(input, columnSpecification);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("hodor", result.First());
        Assert.AreEqual("tenchars10", result.Last());
    }
}

[TestFixture]
public class ParseTests
{
    [Test]
    public void Parse_DataWithHeader_Test()
    {
        string fileContent =
            @"Name    Street    StartDate
Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";

        var columnSpecs = new ColumnSpecification[] {
            new ColumnSpecification{Length = 8, Type = ColumnType.String },
            new ColumnSpecification{Length = 10, Type = ColumnType.String },
            new ColumnSpecification {Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd" } };

        var input = new Input { ColumnSpecifications = columnSpecs, FlatFileContent = fileContent, HeaderRow = HeaderRowType.FixedWidth };
        var options = new Options { SkipRows = false };

        var result = FixedWidthFlatFile.Parse(input, options);

        Assert.AreEqual(2, result.Data.Count);

        var firstRow = result.Data.First();
        Assert.IsTrue(firstRow.ContainsKey("Name"));
        Assert.AreEqual("Veijo", firstRow["Name"]);
        Assert.IsTrue(firstRow.ContainsKey("Street"));
        Assert.AreEqual("FrendsStr", firstRow["Street"]);
        Assert.IsTrue(firstRow.ContainsKey("StartDate"));
    }

    [Test]
    public void Parse_DataWithDelimitedHeader_Test()
    {
        string fileContent =
            @"Name;Street;StartDate
Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";

        var columnSpecs = new ColumnSpecification[] {
            new ColumnSpecification{Length = 8, Type = ColumnType.String },
            new ColumnSpecification{Length = 10, Type = ColumnType.String },
            new ColumnSpecification {Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd" } };

        var input = new Input { ColumnSpecifications = columnSpecs, FlatFileContent = fileContent, HeaderRow = HeaderRowType.Delimited, HeaderDelimiter = ";" };
        var options = new Options { SkipRows = false };

        var result = FixedWidthFlatFile.Parse(input, options);

        Assert.AreEqual(2, result.Data.Count);

        var firstRow = result.Data.First();
        Assert.IsTrue(firstRow.ContainsKey("Name"));
        Assert.AreEqual("Veijo", firstRow["Name"]);
        Assert.IsTrue(firstRow.ContainsKey("Street"));
        Assert.AreEqual("FrendsStr", firstRow["Street"]);
        Assert.IsTrue(firstRow.ContainsKey("StartDate"));
    }

    [Test]
    public void Parse_WithoutHeader_Test()
    {
        string fileContent = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";

        var columnSpecs = new ColumnSpecification[] {
            new ColumnSpecification{Length = 8, Type = ColumnType.String, Name = "Name" },
            new ColumnSpecification{Length = 10, Type = ColumnType.String, Name ="Street" },
            new ColumnSpecification {Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Name = "StartDate" } };

        var input = new Input { ColumnSpecifications = columnSpecs, FlatFileContent = fileContent, HeaderRow = HeaderRowType.None };
        var options = new Options { SkipRows = false };

        var result = FixedWidthFlatFile.Parse(input, options);

        Assert.AreEqual(2, result.Data.Count);

        var firstRow = result.Data.First();
        Assert.IsTrue(firstRow.ContainsKey("Name"));
        Assert.AreEqual("Veijo", firstRow["Name"]);
        Assert.IsTrue(firstRow.ContainsKey("Street"));
        Assert.AreEqual("FrendsStr", firstRow["Street"]);
        Assert.IsTrue(firstRow.ContainsKey("StartDate"));
    }

    [Test]
    public void Parse_AddsGenericKeys_ForValuesWithoutName()
    {
        string fileContent = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 " + System.Environment.NewLine;

        var columnSpecs = new ColumnSpecification[] {
            new ColumnSpecification{Length = 8, Type = ColumnType.String, Name = "Name" },
            new ColumnSpecification{Length = 10, Type = ColumnType.String },
            new ColumnSpecification {Length = 9, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd"} };

        var input = new Input { ColumnSpecifications = columnSpecs, FlatFileContent = fileContent, HeaderRow = HeaderRowType.None };
        var options = new Options { SkipRows = false };

        var result = FixedWidthFlatFile.Parse(input, options);

        Assert.AreEqual(2, result.Data.Count);

        var firstRow = result.Data.First();
        Assert.IsTrue(firstRow.ContainsKey("Name"));
        Assert.AreEqual("Veijo", firstRow["Name"]);
        Assert.IsTrue(firstRow.ContainsKey("Field_2"));
        Assert.AreEqual("FrendsStr", firstRow["Field_2"]);
        Assert.IsTrue(firstRow.ContainsKey("Field_3"));
    }

    [Test]
    public void ParseDataRow_CreatesFieldNames()
    {
        var testData = "firstValuesecondValuethirdValueTrue12.1";
        var columns = new ColumnSpecification[]
        {
                new ColumnSpecification{Type=ColumnType.String, Length=10},
                new ColumnSpecification{Type=ColumnType.String, Length = 11},
                new ColumnSpecification{Type=ColumnType.String, Length = 10},
                new ColumnSpecification{Type = ColumnType.Boolean, Length=4},
                new ColumnSpecification{Type = ColumnType.Double, Length=4}
        };
        var result = FixedWidthFlatFile.ParseDataRow(testData, columns);

        Assert.AreEqual(5, result.Count);
        Assert.AreEqual("Field_1", result.Keys.First());
        Assert.AreEqual("Field_5", result.Keys.Last());
    }

    [Test]
    public void ParseDataRow_TypeCasting_Test()
    {
        var testData = "truet1,51234567890201804249";
        var columns = new ColumnSpecification[] {
                new ColumnSpecification{Type = ColumnType.Boolean, Length = 4, Name = "Boolean"},
                new ColumnSpecification{Type = ColumnType.Char, Length = 1, Name = "Char"},
                new ColumnSpecification{Type = ColumnType.Double, Length = 3, Name = "Double"},
                new ColumnSpecification{Type = ColumnType.Long, Length = 10, Name = "Long"},
                new ColumnSpecification{Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 8, Name = "DateTime"},
                new ColumnSpecification{Type = ColumnType.Int, Length = 1, Name = "Int"}
            };
        var result = FixedWidthFlatFile.ParseDataRow(testData, columns);

        Assert.AreEqual(6, result.Count);
        Assert.AreEqual(typeof(bool), result["Boolean"]?.GetType());
        Assert.IsTrue(result["Char"] is Char);
        Assert.AreEqual(typeof(double), result["Double"]?.GetType());
        Assert.AreEqual(typeof(long), result["Long"]?.GetType());
        Assert.AreEqual(typeof(DateTime), result["DateTime"]?.GetType());
        Assert.AreEqual(typeof(int), result["Int"]?.GetType());
    }
}