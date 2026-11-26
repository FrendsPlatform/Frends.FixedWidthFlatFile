using System.IO;
using Frends.FixedWidthFlatFile.CreateFromObject.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.CreateFromObject.Tests;

[TestFixture]
internal class FunctionalTests
{
    [Test]
    public async Task WritesFixedWidthHeaderAndTypedValues()
    {
        var input = new Input
        {
            Data =
            [
                new Dictionary<string, object>
                {
                    ["Name"] = "Veijo",
                    ["Date"] = new DateTime(2018, 5, 27),
                    ["Amount"] = 12.5m,
                },
            ],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.FixedWidth,
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "Name", Length = 8 },
                new ColumnSpecification
                {
                    Name = "Date",
                    Type = ColumnType.DateTime,
                    DateTimeFormat = "yyyyMMdd",
                    Length = 9,
                },
                new ColumnSpecification { Name = "Amount", Type = ColumnType.Decimal, Length = 6 },
            ],
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(result.FlatFileContent, Is.EqualTo(
            $"Name    Date     Amount{Environment.NewLine}Veijo   20180527 12.5  "));
    }

    [Test]
    public async Task WritesDelimitedHeaderAndRowsWithoutPaddingHeader()
    {
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Id"] = 7, ["Code"] = "AB" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.Delimited,
            HeaderDelimiter = ";",
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "Id", Type = ColumnType.Int, Length = 3 },
                new ColumnSpecification { Name = "Code", Length = 4 },
            ],
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo($"Id;Code{Environment.NewLine}7  AB  "));
    }

    [Test]
    public async Task WritesNoHeaderAndUsesGeneratedNamesForUnnamedColumns()
    {
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Field_1"] = "x" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Length = 2 }],
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo("x "));
    }

    [Test]
    public async Task SkipsConfiguredRowsFromBothEnds()
    {
        var input = new Input
        {
            Data =
            [
                new Dictionary<string, object> { ["Value"] = "a" },
                new Dictionary<string, object> { ["Value"] = "b" },
                new Dictionary<string, object> { ["Value"] = "c" },
            ],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Value", Length = 1 }],
            SkipRows = true,
            SkipRowsFromTop = 1,
            SkipRowsFromBottom = 1,
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo("b"));
    }

    [Test]
    public async Task ReturnsFailureWhenAValueExceedsItsWidth()
    {
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Name"] = "too long" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Name", Length = 3 }],
            ThrowErrorOnFailure = false,
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("exceeds configured width"));
        Assert.That(result.Error.AdditionalInfo, Is.TypeOf<InvalidDataException>());
    }

    [Test]
    public async Task ThrowsWhenDataDoesNotContainConfiguredColumn()
    {
        var input = new Input
        {
            Data = [new Dictionary<string, object>()],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Missing", Length = 3 }],
        };

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None));
    }

    [Test]
    public async Task PropagatesCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await FixedWidthFlatFile.CreateFromObject(
                new Input(),
                new Options
                {
                    ColumnSpecifications = [new ColumnSpecification { Name = "Value", Length = 1 }],
                },
                cancellationSource.Token));
    }

    [Test]
    public async Task WritesContentToNewFileWhenDestinationIsFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var destinationPath = Path.Combine(directory, "output.txt");
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Value"] = "abc" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Value", Length = 4 }],
            DestinationType = DataLocation.File,
            DestinationFilePath = destinationPath,
        };

        try
        {
            var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.FlatFileContent, Is.Null);
            Assert.That(result.FilePath, Is.EqualTo(destinationPath));
            Assert.That(await File.ReadAllTextAsync(destinationPath), Is.EqualTo("abc "));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }

    [Test]
    public async Task DoesNotOverwriteExistingDestinationFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var destinationPath = Path.Combine(directory, "output.txt");
        await File.WriteAllTextAsync(destinationPath, "existing");
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Value"] = "new" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Value", Length = 3 }],
            DestinationType = DataLocation.File,
            DestinationFilePath = destinationPath,
            ThrowErrorOnFailure = false,
        };

        try
        {
            var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(await File.ReadAllTextAsync(destinationPath), Is.EqualTo("existing"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task RequiresDestinationPathForFileOutput()
    {
        var input = new Input
        {
            Data = [new Dictionary<string, object> { ["Value"] = "new" }],
        };
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "Value", Length = 3 }],
            DestinationType = DataLocation.File,
            ThrowErrorOnFailure = false,
        };

        var result = await FixedWidthFlatFile.CreateFromObject(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("DestinationFilePath is required"));
    }
}
