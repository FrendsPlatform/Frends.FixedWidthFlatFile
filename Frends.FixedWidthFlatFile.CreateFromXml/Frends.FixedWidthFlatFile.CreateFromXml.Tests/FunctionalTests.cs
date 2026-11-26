using Frends.FixedWidthFlatFile.CreateFromXml.Definitions;
using NUnit.Framework;
using ToXmlDefs = Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using ToXmlTask = Frends.FixedWidthFlatFile.ConvertToXML.FixedWidthFlatFile;

namespace Frends.FixedWidthFlatFile.CreateFromXml.Tests;

[TestFixture]
internal class FunctionalTests
{
    private const string FlatFile =
        "Name    Street    StartDate Salary Active\r\n" +
        "Veijo   FrendsStr 20180527  1234.5 True  \r\n" +
        "Hodor   HodorsStr 20180101  99     False ";

    private const string Xml =
        "<Root><Rows>" +
        "<Row><Name>Veijo</Name><Street>FrendsStr</Street><StartDate>2018-05-27T00:00:00</StartDate><Salary>1234.5</Salary><Active>True</Active></Row>" +
        "<Row><Name>Hodor</Name><Street></Street><StartDate>2018-01-01T00:00:00</StartDate><Salary>99</Salary><Active>False</Active></Row>" +
        "</Rows></Root>";

    private string tempDir;

    [SetUp]
    public void SetUp()
    {
        tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [Test]
    public void CreatesFixedWidthFileFromXml()
    {
        var result = Run(new Input { XmlContent = Xml }, DefaultOptions());

        Assert.That(result.Success, Is.True);
        var lines = result.Data.Split(Environment.NewLine);
        Assert.That(lines, Has.Length.EqualTo(3));
        Assert.That(lines[0], Is.EqualTo("Name    Street    StartDate Salary Active"));
        Assert.That(lines[2], Is.EqualTo("Hodor             20180101  99     False "));
    }

    [Test]
    public void RoundTripFixedWidthHeaderReturnsOriginalFile()
    {
        var original = FlatFile.Replace("\r\n", Environment.NewLine);
        var options = DefaultOptions();

        var xml = ToXml(original, options);
        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Data, Is.EqualTo(original));
    }

    [Test]
    public void RoundTripWithoutHeaderReturnsOriginalFile()
    {
        var original = string.Join(Environment.NewLine, FlatFile.Split("\r\n").Skip(1));
        var options = DefaultOptions();
        options.HeaderRow = HeaderRowType.None;

        var xml = ToXml(original, options);
        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Data, Is.EqualTo(original));
    }

    [Test]
    public void RoundTripDelimitedHeaderReturnsOriginalFile()
    {
        var original = "Name;Street;StartDate;Salary;Active" + Environment.NewLine +
                       string.Join(Environment.NewLine, FlatFile.Split("\r\n").Skip(1));
        var options = DefaultOptions();
        options.HeaderRow = HeaderRowType.Delimited;
        options.HeaderDelimiter = ";";
        foreach (var spec in options.ColumnSpecifications)
            spec.Name = string.Empty;

        var xml = ToXml(original, options);
        var xmlOptions = DefaultOptions();
        xmlOptions.HeaderRow = HeaderRowType.Delimited;
        xmlOptions.HeaderDelimiter = ";";
        foreach (var spec in xmlOptions.ColumnSpecifications)
            spec.Name = string.Empty;

        var result = Run(new Input { XmlContent = xml }, xmlOptions);

        Assert.That(result.Data, Is.EqualTo(original));
    }

    [Test]
    public void RoundTripWithCultureReturnsOriginalFile()
    {
        var original = "Price   Day       " + Environment.NewLine + "1234,50 27.05.2018";
        var options = new Options
        {
            HeaderRow = HeaderRowType.FixedWidth,
            Culture = "fi-FI",
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "Price", Type = ColumnType.Decimal, Length = 8 },
                new ColumnSpecification { Name = "Day", Type = ColumnType.DateTime, DateTimeFormat = "dd.MM.yyyy", Length = 10 },
            ],
        };

        var xml = ToXml(original, options);
        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Data, Is.EqualTo(original));
    }

    [Test]
    public void NonBreakingSpaceInStringAndCharColumnsIsPreserved()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "A", Type = ColumnType.String, Length = 1 },
                new ColumnSpecification { Name = "B", Type = ColumnType.Char, Length = 1 },
            ],
        };

        var xml = "<Root><Rows><Row><A>&#xA0;</A><B>&#xA0;</B></Row></Rows></Root>";
        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Data, Is.EqualTo("\u00A0\u00A0"));
    }

    [Test]
    public void EmptyDateTimeFormatUsesDefaultFormat()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "D", Type = ColumnType.DateTime, DateTimeFormat = string.Empty, Length = 19 },
            ],
        };

        var xml = "<Root><Rows><Row><D>2018-05-27T13:14:15</D></Row></Rows></Root>";
        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Data, Is.EqualTo("2018-05-27 13:14:15"));
    }

    [Test]
    public void DelimitedHeaderWithLineBreakFails()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = "\n",
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "A", Type = ColumnType.String, Length = 1 },
                new ColumnSpecification { Name = "B", Type = ColumnType.String, Length = 1 },
            ],
        };

        var xml = "<Root><Rows><Row><A>1</A><B>2</B></Row></Rows></Root>";
        Assert.Throws<InvalidDataException>(() => Run(new Input { XmlContent = xml }, options));
    }

    [Test]
    public void RoundTripThroughFilesReturnsOriginalFile()
    {
        var original = FlatFile.Replace("\r\n", Environment.NewLine);
        var xmlPath = Path.Combine(tempDir, "data.xml");
        var flatPath = Path.Combine(tempDir, "sub", "data.txt");

        var toXml = ToXmlTask.ConvertToXML(
            new ToXmlDefs.Input { FlatFileContent = original },
            ToXmlOptions(DefaultOptions(), xmlPath),
            CancellationToken.None);
        Assert.That(toXml.Success, Is.True);

        var options = DefaultOptions();
        options.DestinationType = DataLocation.File;
        options.DestinationFilePath = flatPath;
        var result = Run(new Input { SourceType = DataLocation.File, XmlFilePath = xmlPath }, options);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.FilePath, Is.EqualTo(flatPath));
        Assert.That(File.ReadAllText(flatPath), Is.EqualTo(original));
    }

    [Test]
    public void FailsWhenValueIsLongerThanColumn()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        options.ColumnSpecifications[0].Length = 3;

        var result = Run(new Input { XmlContent = Xml }, options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("longer than the column length"));
    }

    [Test]
    public void FailsOnInvalidTypedValue()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Run(new Input { XmlContent = Xml.Replace("1234.5", "abc") }, options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("cannot be converted"));
    }

    [Test]
    public void FailsOnWrongStructure()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Run(new Input { XmlContent = "<Root><Other/></Root>" }, options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Root/Rows/Row"));
    }

    [Test]
    public void RejectsDtd()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var xml = "<!DOCTYPE Root [<!ENTITY x \"y\">]><Root><Rows/></Root>";

        var result = Run(new Input { XmlContent = xml }, options);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void FailsWhenSourceFileIsMissing()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = Run(new Input { SourceType = DataLocation.File, XmlFilePath = Path.Combine(tempDir, "no.xml") }, options);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<FileNotFoundException>());
    }

    [Test]
    public void DoesNotOverwriteExistingDestination()
    {
        var path = Path.Combine(tempDir, "exists.txt");
        File.WriteAllText(path, "old");
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        options.DestinationType = DataLocation.File;
        options.DestinationFilePath = path;

        var result = Run(new Input { XmlContent = Xml }, options);

        Assert.That(result.Success, Is.False);
        Assert.That(File.ReadAllText(path), Is.EqualTo("old"));
    }

    [Test]
    public void ThrowsWhenThrowErrorOnFailureIsTrue()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = "Custom";

        var ex = Assert.Throws<Exception>(() => Run(new Input { XmlContent = "<Root/>" }, options));

        Assert.That(ex.Message, Is.EqualTo("Custom"));
    }

    [Test]
    public void ThrowsWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            FixedWidthFlatFile.CreateFromXml(new Input { XmlContent = Xml }, DefaultOptions(), cts.Token));
    }

    private static Result Run(Input input, Options options) =>
        FixedWidthFlatFile.CreateFromXml(input, options, CancellationToken.None);

    private static string ToXml(string flatFile, Options options)
    {
        var result = ToXmlTask.ConvertToXML(
            new ToXmlDefs.Input { FlatFileContent = flatFile },
            ToXmlOptions(options, null),
            CancellationToken.None);
        Assert.That(result.Success, Is.True, result.Error?.Message);
        return result.Data;
    }

    private static ToXmlDefs.Options ToXmlOptions(Options options, string destinationPath) => new()
    {
        HeaderRow = Enum.Parse<ToXmlDefs.HeaderRowType>(options.HeaderRow.ToString()),
        HeaderDelimiter = options.HeaderDelimiter,
        Culture = options.Culture,
        DestinationType = destinationPath == null ? ToXmlDefs.DataLocation.String : ToXmlDefs.DataLocation.File,
        DestinationFilePath = destinationPath,
        ColumnSpecifications =
        [
            .. options.ColumnSpecifications.Select(c => new ToXmlDefs.ColumnSpecification
            {
                Name = c.Name,
                Type = Enum.Parse<ToXmlDefs.ColumnType>(c.Type.ToString()),
                DateTimeFormat = c.DateTimeFormat,
                Length = c.Length,
            }),
        ],
    };

    private static Options DefaultOptions() => new()
    {
        HeaderRow = HeaderRowType.FixedWidth,
        ColumnSpecifications =
        [
            new ColumnSpecification { Name = "Name", Type = ColumnType.String, Length = 8 },
            new ColumnSpecification { Name = "Street", Type = ColumnType.String, Length = 10 },
            new ColumnSpecification { Name = "StartDate", Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 10 },
            new ColumnSpecification { Name = "Salary", Type = ColumnType.Decimal, Length = 7 },
            new ColumnSpecification { Name = "Active", Type = ColumnType.Boolean, Length = 6 },
        ],
    };
}
