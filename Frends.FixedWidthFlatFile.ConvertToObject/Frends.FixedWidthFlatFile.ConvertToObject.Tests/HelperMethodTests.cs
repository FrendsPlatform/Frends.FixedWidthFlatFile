using Frends.FixedWidthFlatFile.ConvertToObject.Definitions;
using Frends.FixedWidthFlatFile.ConvertToObject.Helpers;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToObject.Tests;

[TestFixture]
internal class HelperMethodTests
{
    [Test]
    public void SplitToList_WithDelimiter_Test()
    {
        var result = FlatFileParser.SplitToList("h1;h2;h3;h4,h5", ';');

        Assert.That(result, Has.Count.EqualTo(4));
        Assert.That(result.Last(), Is.EqualTo("h4,h5"));
    }

    [Test]
    public void SplitToList_FixedWidth_Test()
    {
        var columnSpecification = new[]
        {
            new ColumnSpecification { Name = "h1", Length = 5, Type = ColumnType.String },
            new ColumnSpecification { Name = "h2", Length = 2, Type = ColumnType.Int },
            new ColumnSpecification { Name = "h3", Length = 10, Type = ColumnType.String },
        };

        var result = FlatFileParser.SplitToList("hodor22tenchars10", columnSpecification);

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.First(), Is.EqualTo("hodor"));
        Assert.That(result.Last(), Is.EqualTo("tenchars10"));
    }

    [Test]
    public void ParseDataRow_CreatesFieldNames()
    {
        var columns = new[]
        {
            new ColumnSpecification { Type = ColumnType.String, Length = 10 },
            new ColumnSpecification { Type = ColumnType.String, Length = 11 },
            new ColumnSpecification { Type = ColumnType.String, Length = 10 },
            new ColumnSpecification { Type = ColumnType.Boolean, Length = 4 },
            new ColumnSpecification { Type = ColumnType.Double, Length = 4 },
        };

        var result = FlatFileParser.ParseDataRow("firstValuesecondValuethirdValueTrue12.1", columns);

        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result.Keys.First(), Is.EqualTo("Field_1"));
        Assert.That(result.Keys.Last(), Is.EqualTo("Field_5"));
    }

    [Test]
    public void ParseDataRow_TypeCasting_Test()
    {
        var columns = new[]
        {
            new ColumnSpecification { Type = ColumnType.Boolean, Length = 4, Name = "Boolean" },
            new ColumnSpecification { Type = ColumnType.Char, Length = 1, Name = "Char" },
            new ColumnSpecification { Type = ColumnType.Double, Length = 3, Name = "Double" },
            new ColumnSpecification { Type = ColumnType.Long, Length = 10, Name = "Long" },
            new ColumnSpecification { Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 8, Name = "DateTime" },
            new ColumnSpecification { Type = ColumnType.Int, Length = 1, Name = "Int" },
        };

        var result = FlatFileParser.ParseDataRow("truet1,51234567890201804249", columns);

        Assert.That(result, Has.Count.EqualTo(6));
        Assert.That(result["Boolean"], Is.TypeOf<bool>());
        Assert.That(result["Char"], Is.TypeOf<char>());
        Assert.That(result["Double"], Is.TypeOf<double>());
        Assert.That(result["Long"], Is.TypeOf<long>());
        Assert.That(result["DateTime"], Is.TypeOf<DateTime>());
        Assert.That(result["Int"], Is.TypeOf<int>());
    }

    [Test]
    public void ParseDataRow_Decimal_Test()
    {
        var columns = new[]
        {
            new ColumnSpecification { Type = ColumnType.Decimal, Length = 3, Name = "Dot" },
            new ColumnSpecification { Type = ColumnType.Decimal, Length = 3, Name = "Comma" },
        };

        var result = FlatFileParser.ParseDataRow("1.52,5", columns);

        Assert.That(result["Dot"], Is.EqualTo(1.5m));
        Assert.That(result["Comma"], Is.EqualTo(2.5m));
    }

    [Test]
    public void ParseDataRow_EmptyValue_IsNull()
    {
        var columns = new[]
        {
            new ColumnSpecification { Type = ColumnType.Int, Length = 3, Name = "Empty" },
            new ColumnSpecification { Type = ColumnType.Int, Length = 1, Name = "Value" },
        };

        var result = FlatFileParser.ParseDataRow("   7", columns);

        Assert.That(result["Empty"], Is.Null);
        Assert.That(result["Value"], Is.EqualTo(7));
    }

    [Test]
    public void AddKeyValuePair_RenamesDuplicateKeys()
    {
        var dictionary = new Dictionary<string, dynamic>();

        dictionary.AddKeyValuePair("Key", 1);
        dictionary.AddKeyValuePair("Key", 2);
        dictionary.AddKeyValuePair("Key", 3);

        Assert.That(dictionary.Keys, Is.EqualTo(["Key", "Key_1", "Key_2"]));
    }
}
