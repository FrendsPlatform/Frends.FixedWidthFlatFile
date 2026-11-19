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
        options.ColumnSpecifications =
        [
            new ColumnSpecification { Name = "Name", Length = 6 },
            new ColumnSpecification { Name = "Street", Length = 9 }
        ];
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
        AssertExpectedRows(JArray.Parse(result.Data));
        Assert.That(result.FilePath, Is.Null);
    }

    private static void AssertExpectedRows(JArray json)
    {
        Assert.That(json.Count, Is.EqualTo(2));
        Assert.That((string)json[0]["Name"], Is.EqualTo("Veijo"));
        Assert.That((string)json[0]["Street"], Is.EqualTo("FrendsStr"));
        Assert.That((string)json[1]["Name"], Is.EqualTo("Hodor"));
        Assert.That((string)json[1]["Street"], Is.EqualTo("HodorsStr"));
    }

    [Test]
    public void ReadSource_InvalidUtf8_Fails()
    {
        var path = Path.Combine(_dir, "in.txt");
        File.WriteAllBytes(path, System.Text.Encoding.Latin1.GetBytes("Name  Street   \nJos\u00e9  FrendsStr\n"));
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = path;

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { ThrowErrorOnFailure = false }), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("not valid UTF-8"));
    }

    [Test]
    public void ReadSource_Utf8WithNonAsciiCharacters_Succeeds()
    {
        var path = Path.Combine(_dir, "in.txt");
        File.WriteAllText(path, "Name  Street   \nJos\u00e9  FrendsStr\n");
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = path;

        var result = FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options()), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That((string)JArray.Parse(result.Data)[0]["Name"], Is.EqualTo("Jos\u00e9"));
    }

    [Test]
    public void ReadSource_CanceledToken_DoesNotReadFile()
    {
        var input = NewInput();
        input.SourceType = DataLocation.File;
        input.FlatFilePath = Path.Combine(_dir, "missing.txt");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => FixedWidthFlatFile.ConvertToJSON(input, WithSpecs(new Options { ThrowErrorOnFailure = false }), cts.Token);
        Assert.Throws<OperationCanceledException>(act);
    }

    [Test]
    public void WriteNewFile_CanceledBeforePublish_LeavesNoFiles()
    {
        var path = Path.Combine(_dir, "out.json");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => Helpers.FileHandler.WriteNewFile(path, "{}", cts.Token);
        Assert.Throws<OperationCanceledException>(act);

        Assert.That(Directory.GetFileSystemEntries(_dir), Is.Empty);
    }

    [Test]
    public void WriteNewFile_ConcurrentPublishToSamePath_ExactlyOneSucceeds()
    {
        var path = Path.Combine(_dir, "out.json");
        var successes = 0;

        System.Threading.Tasks.Parallel.For(0, 8, i =>
        {
            try
            {
                Helpers.FileHandler.WriteNewFile(path, $"[{i}]", CancellationToken.None);
                Interlocked.Increment(ref successes);
            }
            catch (IOException)
            {
            }
        });

        Assert.That(successes, Is.EqualTo(1));
        Assert.That(Directory.GetFiles(_dir), Has.Length.EqualTo(1));
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
        AssertExpectedRows(JArray.Parse(File.ReadAllText(path)));
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
