using Frends.FixedWidthFlatFile.ConvertToObject.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToObject.Tests;

[TestFixture]
internal class ConvertToObjectTests
{
    [Test]
    public void ConvertToObject_DataWithFixedWidthHeader_Test()
    {
        const string content = @"Name    Street    StartDate
Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";
        var input = new Input { FlatFileContent = content };
        var options = new Options
        {
            ColumnSpecifications = NamelessSpecs(),
            HeaderRow = HeaderRowType.FixedWidth,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        AssertFirstRow(result);
    }

    [Test]
    public void ConvertToObject_ReusedOptionsUsesHeadersPerInvocation()
    {
        var input = new Input { FlatFileContent = "Old" + Environment.NewLine + "AAA" };
        var specifications = new[] { new ColumnSpecification { Length = 3, Type = ColumnType.String } };
        var options = new Options
        {
            ColumnSpecifications = specifications,
            HeaderRow = HeaderRowType.FixedWidth,
        };

        var firstResult = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);
        input.FlatFileContent = "New" + Environment.NewLine + "BBB";
        var secondResult = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(firstResult.Data[0]["Old"], Is.EqualTo("AAA"));
        Assert.That(secondResult.Data[0]["New"], Is.EqualTo("BBB"));
        Assert.That(specifications[0].Name, Is.Empty);
    }

    [Test]
    public void ConvertToObject_ReadsContentFromFile_Test()
    {
        const string content = @"Name    Street    StartDate
Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        try
        {
            var input = new Input
            {
                SourceType = DataLocation.File,
                FlatFilePath = path,
            };
            var options = new Options { ColumnSpecifications = NamelessSpecs() };

            var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

            AssertFirstRow(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ConvertToObject_MissingFile_Fails()
    {
        var input = new Input
        {
            SourceType = DataLocation.File,
            FlatFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt"),
        };
        var options = new Options
        {
            ColumnSpecifications = NamelessSpecs(),
            ThrowErrorOnFailure = false,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void ConvertToObject_FileSourceWithoutPath_FailsValidation()
    {
        var input = new Input { SourceType = DataLocation.File };
        var options = new Options
        {
            ColumnSpecifications = NamelessSpecs(),
            ThrowErrorOnFailure = false,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("FlatFilePath"));
    }

    [Test]
    public void ConvertToObject_NoColumnSpecifications_FailsValidation()
    {
        var input = new Input { FlatFileContent = "abc" };
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("ColumnSpecifications"));
    }

    [Test]
    public void ConvertToObject_DataWithDelimitedHeader_Test()
    {
        const string content = @"Name;Street;StartDate
Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";
        var input = new Input { FlatFileContent = content };
        var options = new Options
        {
            ColumnSpecifications = NamelessSpecs(),
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = ";",
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        AssertFirstRow(result);
    }

    [Test]
    public void ConvertToObject_WithoutHeader_Test()
    {
        const string content = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";
        var input = new Input { FlatFileContent = content };
        var options = new Options
        {
            ColumnSpecifications = NamedSpecs(),
            HeaderRow = HeaderRowType.None,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        AssertFirstRow(result);
    }

    [Test]
    public void ConvertToObject_AddsGenericKeys_ForValuesWithoutName()
    {
        var content = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 " + Environment.NewLine;
        var specs = NamedSpecs();
        specs[1].Name = string.Empty;
        specs[2].Name = string.Empty;
        var input = new Input { FlatFileContent = content };
        var options = new Options
        {
            ColumnSpecifications = specs,
            HeaderRow = HeaderRowType.None,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Data, Has.Count.EqualTo(2));
        var firstRow = result.Data.First();
        Assert.That(firstRow["Name"], Is.EqualTo("Veijo"));
        Assert.That(firstRow["Field_2"], Is.EqualTo("FrendsStr"));
        Assert.That(firstRow.ContainsKey("Field_3"), Is.True);
    }

    [Test]
    public void ConvertToObject_SkipRows_Test()
    {
        const string content = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 
Third   ThirdStr  20190101 
Fourth  FourthStr 20200101 ";
        var input = new Input { FlatFileContent = content };
        var options = new Options
        {
            ColumnSpecifications = NamedSpecs(),
            HeaderRow = HeaderRowType.None,
            SkipRows = true,
            SkipRowsFromTop = 1,
            SkipRowsFromBottom = 1,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Data, Has.Count.EqualTo(2));
        Assert.That(result.Data[0]["Name"], Is.EqualTo("Hodor"));
        Assert.That(result.Data[1]["Name"], Is.EqualTo("Third"));
    }

    [Test]
    public void ConvertToObject_SkipMoreRowsThanExist_ReturnsEmpty()
    {
        const string content = @"Veijo   FrendsStr 20180527 
Hodor   HodorsStr 20180101 ";
        var input = new Input { FlatFileContent = content };

        var top = FixedWidthFlatFile.ConvertToObject(
            input,
            new Options
            {
                ColumnSpecifications = NamedSpecs(),
                HeaderRow = HeaderRowType.None,
                SkipRows = true,
                SkipRowsFromTop = 5,
            },
            CancellationToken.None);
        var bottom = FixedWidthFlatFile.ConvertToObject(
            input,
            new Options
            {
                ColumnSpecifications = NamedSpecs(),
                HeaderRow = HeaderRowType.None,
                SkipRows = true,
                SkipRowsFromBottom = 5,
            },
            CancellationToken.None);

        Assert.That(top.Success, Is.True);
        Assert.That(top.Data, Is.Empty);
        Assert.That(bottom.Success, Is.True);
        Assert.That(bottom.Data, Is.Empty);
    }

    private static ColumnSpecification[] NamelessSpecs() =>
    [
        new()
        {
            Length = 8,
            Type = ColumnType.String,
        },
        new()
        {
            Length = 10,
            Type = ColumnType.String,
        },
        new()
        {
            Length = 9,
            Type = ColumnType.DateTime,
            DateTimeFormat = "yyyyMMdd",
        },
    ];

    private static ColumnSpecification[] NamedSpecs() =>
    [
        new()
        {
            Length = 8,
            Type = ColumnType.String,
            Name = "Name",
        },
        new()
        {
            Length = 10,
            Type = ColumnType.String,
            Name = "Street",
        },
        new()
        {
            Length = 9,
            Type = ColumnType.DateTime,
            DateTimeFormat = "yyyyMMdd",
            Name = "StartDate",
        },
    ];

    private static void AssertFirstRow(Result result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(result.Data, Has.Count.EqualTo(2));

        var firstRow = result.Data.First();
        Assert.That(firstRow["Name"], Is.EqualTo("Veijo"));
        Assert.That(firstRow["Street"], Is.EqualTo("FrendsStr"));
        Assert.That(firstRow["StartDate"], Is.EqualTo(new DateTime(2018, 5, 27)));
    }
}
