using System;
using System.IO;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToXML.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToXML.Tests;

[TestFixture]
internal class SourceAndDestinationTests
{
    private string dir;

    private static Input NewInput() => new()
    {
        HeaderRow = HeaderRowType.None,
        ColumnSpecifications =
        [
            new ColumnSpecification { Name = "Name", Length = 6 },
            new ColumnSpecification { Name = "Street", Length = 5 },
        ],
    };

    [SetUp]
    public void SetUp()
    {
        dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(dir, true);

    [Test]
    public void Reads_From_File_And_Returns_String()
    {
        var path = Path.Combine(dir, "in.txt");
        File.WriteAllText(path, "Veijo Frend");
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = path;

        var result = FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Does.Contain("<Name>Veijo</Name>"));
        Assert.That(result.FilePath, Is.Null);
    }

    [Test]
    public void Writes_Xml_To_File()
    {
        var path = Path.Combine(dir, "sub", "out.xml");
        var input = NewInput();
        input.FlatFileContent = "Veijo Frend";
        var options = new Options { DestinationType = DataLocation.File, DestinationFilePath = path };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.FilePath, Is.EqualTo(path));
        Assert.That(File.ReadAllText(path), Does.Contain("<Street>Frend</Street>"));
    }

    [Test]
    public void Does_Not_Overwrite_Existing_Destination()
    {
        var path = Path.Combine(dir, "out.xml");
        File.WriteAllText(path, "x");
        var input = NewInput();
        input.FlatFileContent = "Veijo Frend";
        var options = new Options { DestinationType = DataLocation.File, DestinationFilePath = path, ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(File.ReadAllText(path), Is.EqualTo("x"));
    }

    [Test]
    public void Missing_Source_File_Path_Fails_Validation()
    {
        var input = NewInput();
        input.SourceType = DataLocation.File;
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.ConvertToXML(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Validation failed"));
    }

    [Test]
    public void Missing_Source_File_Fails()
    {
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = Path.Combine(dir, "nope.txt");

        Assert.Throws<FileNotFoundException>((Action)(() =>
            FixedWidthFlatFile.ConvertToXML(input, new Options(), CancellationToken.None)));
    }
}
