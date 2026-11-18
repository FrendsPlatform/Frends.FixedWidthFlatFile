using System;
using System.IO;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests;

[TestFixture]
public class SourceAndDestinationTests
{
    private const string Content = "Name  Street   \nVeijo FrendsStr\nHodor HodorsStr\n";

    private string _dir;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_dir, true);

    private static Input NewInput() => new();

    private static Options WithSpecs(Options options)
    {
        options.HeaderRow = HeaderRowType.FixedWidth;
        options.ColumnSpecifications = new[]
        {
            new ColumnSpecification { Name = "Name", Length = 6 },
            new ColumnSpecification { Name = "Street", Length = 9 },
        };
        return options;
    }

    [Test]
    public void ReadsFromFile()
    {
        var path = Path.Combine(_dir, "in.txt");
        File.WriteAllText(path, Content);
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = path;

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options()), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(JArray.Parse(result.Data).Count, Is.EqualTo(2));
        Assert.That(result.FilePath, Is.Null);
    }

    [Test]
    public void MissingSourceFileFails()
    {
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = Path.Combine(_dir, "missing.txt");

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { ThrowErrorOnFailure = false }), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("was not found"));
    }

    [Test]
    public void SourceFileRequiresPath()
    {
        var input = NewInput();
        input.SourceType = DataLocation.File;

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { ThrowErrorOnFailure = false }), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("FlatFilePath"));
    }

    [Test]
    public void WritesToFile()
    {
        var input = NewInput();
        input.FlatFileContent = Content;
        var path = Path.Combine(_dir, "sub", "out.json");

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { DestinationType = DataLocation.File, DestinationFilePath = path }), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.FilePath, Is.EqualTo(path));
        Assert.That(JArray.Parse(File.ReadAllText(path)).Count, Is.EqualTo(2));
        Assert.That(Directory.GetFiles(Path.GetDirectoryName(path)), Has.Length.EqualTo(1));
    }

    [Test]
    public void DoesNotOverwriteExistingFile()
    {
        var input = NewInput();
        input.FlatFileContent = Content;
        var path = Path.Combine(_dir, "out.json");
        File.WriteAllText(path, "original");

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { DestinationType = DataLocation.File, DestinationFilePath = path, ThrowErrorOnFailure = false }), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(File.ReadAllText(path), Is.EqualTo("original"));
    }

    [Test]
    public void RelativeDestinationPathFails()
    {
        var input = NewInput();
        input.FlatFileContent = Content;

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { DestinationType = DataLocation.File, DestinationFilePath = "out.json", ThrowErrorOnFailure = false }), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("fully qualified"));
    }
}
