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
    public void ConvertToObject_DelimitedHeaderWithMoreColumnsThanSpecifications_FailsClearly()
    {
        var input = new Input { FlatFileContent = "First;Second;Third\n1234" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = ";",
            ColumnSpecifications = [new ColumnSpecification { Length = 4 }],
            ThrowErrorOnFailure = false,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<InvalidDataException>());
        Assert.That(result.Error.Message, Does.Contain("3 columns").And.Contain("1 column specifications"));
    }

    [Test]
    public void ConvertToObject_InvalidCulture_FailsClearly()
    {
        var input = new Input { FlatFileContent = "12", Culture = "!" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 2, Type = ColumnType.Decimal }],
            ThrowErrorOnFailure = false,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<System.Globalization.CultureNotFoundException>());
    }

    [Test]
    public void ConvertToObject_ParsingFailureIncludesDataRowAndColumn()
    {
        var input = new Input { FlatFileContent = "123\nabc" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 3, Type = ColumnType.Int, Name = "Count" }],
            ThrowErrorOnFailure = false,
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("row 2").And.Contain("Count"));
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<InvalidDataException>());
        Assert.That(result.Error.AdditionalInfo.InnerException, Is.TypeOf<FormatException>());
    }

    [Test]
    public void ConvertToObject_ProvidedCulture_ParsesAmount()
    {
        var input = new Input { FlatFileContent = "1,234", Culture = "fi-FI" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 5, Type = ColumnType.Decimal, Name = "Amount" }],
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data[0]["Amount"], Is.EqualTo(1.234m));
    }

    [Test]
    public void ConvertToObject_NoCulture_UsesInvariantCulture()
    {
        var input = new Input { FlatFileContent = "1,234.56" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 8, Type = ColumnType.Decimal, Name = "Amount" }],
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data[0]["Amount"], Is.EqualTo(1234.56m));
    }

    [Test]
    public void ConvertToObject_ProvidedCulture_ParsesDate()
    {
        var input = new Input { FlatFileContent = "31.M\u00E4rz.2025", Culture = "de-DE" };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 12, Type = ColumnType.DateTime, DateTimeFormat = "dd.MMMM.yyyy", Name = "Date" }],
        };

        var result = FixedWidthFlatFile.ConvertToObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data[0]["Date"], Is.EqualTo(new DateTime(2025, 3, 31)));
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
