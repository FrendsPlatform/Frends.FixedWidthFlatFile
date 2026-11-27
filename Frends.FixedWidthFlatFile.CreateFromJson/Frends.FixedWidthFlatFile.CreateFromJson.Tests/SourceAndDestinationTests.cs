using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using Frends.FixedWidthFlatFile.CreateFromJson.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Tests;

[TestFixture]
internal class SourceAndDestinationTests : TestBase
{
    private string tempDir;

    [SetUp]
    public void SetUp() => tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
    }

    [Test]
    public void ReadsJsonFromFile()
    {
        Directory.CreateDirectory(tempDir);
        var source = Path.Combine(tempDir, "in.json");
        File.WriteAllText(source, DefaultInput().Json);
        var input = DefaultInput();
        input.SourceType = DataLocation.File;
        input.Json = null;
        input.JsonFilePath = source;

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo("Veijo   20180527"));
    }

    [Test]
    public void FileSourceRequiresPath()
    {
        var input = DefaultInput();
        input.SourceType = DataLocation.File;

        Assert.Throws<ValidationException>(
            () => FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None));
    }

    [Test]
    public void MissingSourceFileFails()
    {
        var input = DefaultInput();
        input.SourceType = DataLocation.File;
        input.JsonFilePath = Path.Combine(tempDir, "missing.json");

        Assert.Throws<FileNotFoundException>(
            () => FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None));
    }

    [Test]
    public void WritesFileAndCreatesDirectories()
    {
        var path = Path.Combine(tempDir, "a", "b", "out.txt");
        var options = DefaultOptions();
        options.DestinationType = DataLocation.File;
        options.DestinationFilePath = path;

        var result = FixedWidthFlatFile.CreateFromJson(DefaultInput(), options, CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.Null);
        Assert.That(result.FlatFilePath, Is.EqualTo(path));
        Assert.That(File.ReadAllText(path), Is.EqualTo("Veijo   20180527"));
    }

    [Test]
    public void StringDestinationLeavesPathNull()
    {
        var result = FixedWidthFlatFile.CreateFromJson(DefaultInput(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFilePath, Is.Null);
    }

    [Test]
    public void DoesNotOverwriteExistingFile()
    {
        Directory.CreateDirectory(tempDir);
        var path = Path.Combine(tempDir, "out.txt");
        File.WriteAllText(path, "original");
        var options = DefaultOptions();
        options.DestinationType = DataLocation.File;
        options.DestinationFilePath = path;

        Assert.Throws<IOException>(
            () => FixedWidthFlatFile.CreateFromJson(DefaultInput(), options, CancellationToken.None));
        Assert.That(File.ReadAllText(path), Is.EqualTo("original"));
    }

    [Test]
    public void FileDestinationRequiresValidAbsolutePath()
    {
        var options = DefaultOptions();
        options.DestinationType = DataLocation.File;

        options.DestinationFilePath = string.Empty;
        Assert.Throws<ValidationException>(
            () => FixedWidthFlatFile.CreateFromJson(DefaultInput(), options, CancellationToken.None));

        options.DestinationFilePath = "relative.txt";
        Assert.Throws<ArgumentException>(
            () => FixedWidthFlatFile.CreateFromJson(DefaultInput(), options, CancellationToken.None));

        options.DestinationFilePath = tempDir + Path.DirectorySeparatorChar;
        Assert.Throws<ArgumentException>(
            () => FixedWidthFlatFile.CreateFromJson(DefaultInput(), options, CancellationToken.None));
    }
}
