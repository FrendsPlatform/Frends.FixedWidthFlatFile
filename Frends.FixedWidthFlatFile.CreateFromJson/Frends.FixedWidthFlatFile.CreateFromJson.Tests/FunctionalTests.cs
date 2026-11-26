using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Frends.FixedWidthFlatFile.CreateFromJson.Definitions;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Tests;

[TestFixture]
internal class FunctionalTests : TestBase
{
    [Test]
    public void ConvertsJsonArrayToFixedWidthRecords()
    {
        var result = FixedWidthFlatFile.CreateFromJson(DefaultInput(), DefaultOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.Null);
        Assert.That(result.FlatFileContent, Is.EqualTo("Veijo   20180527"));
    }

    [Test]
    public void WritesHeaderWhenRequested()
    {
        var input = DefaultInput();
        input.IncludeHeaderRow = true;
        input.ColumnSpecifications[1].Length = 9;

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo($"Name    StartDate{Environment.NewLine}Veijo   20180527 "));
    }

    [Test]
    public void WritesNullAndMissingValuesAsSpaces()
    {
        var input = new Input
        {
            Json = "[{\"Name\":null},{}]",
            ColumnSpecifications = [new ColumnSpecification { Name = "Name", Length = 4 }],
        };

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo($"    {Environment.NewLine}    "));
    }

    [Test]
    public void FormatsNumbersUsingSelectedCulture()
    {
        var input = new Input
        {
            Json = "[{\"Amount\":12.5}]",
            Culture = "fi-FI",
            ColumnSpecifications = [new ColumnSpecification { Name = "Amount", Type = ColumnType.Decimal, Length = 5 }],
        };

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo("12,5 "));
    }

    [Test]
    public void ReturnsFailureWhenValueDoesNotFitItsColumn()
    {
        var input = new Input
        {
            Json = "[{\"Name\":\"too long\"}]",
            ColumnSpecifications = [new ColumnSpecification { Name = "Name", Length = 4 }],
        };
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Conversion failed",
        };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.StartWith("Conversion failed:"));
        Assert.That(result.Error.Message, Does.Contain("exceeding its width"));
    }

    [Test]
    public void PreservesDecimalPrecision()
    {
        var input = new Input
        {
            Json = "[{\"Amount\":0.1234567890123456789012345678}]",
            ColumnSpecifications = [new ColumnSpecification { Name = "Amount", Type = ColumnType.Decimal, Length = 30 }],
        };

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent.TrimEnd(), Is.EqualTo("0.1234567890123456789012345678"));
    }

    [Test]
    public void ParsesInputWithInvariantCulture()
    {
        var input = new Input
        {
            Json = "[{\"Amount\":\"1.5\"}]",
            Culture = "fi-FI",
            ColumnSpecifications = [new ColumnSpecification { Name = "Amount", Type = ColumnType.Decimal, Length = 4 }],
        };

        var result = FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None);

        Assert.That(result.FlatFileContent, Is.EqualTo("1,5 "));
    }

    [TestCase("1.5", ColumnType.Int)]
    [TestCase("1.5", ColumnType.Long)]
    [TestCase("true", ColumnType.Int)]
    [TestCase("\"abc\"", ColumnType.Int)]
    [TestCase("\"abc\"", ColumnType.Decimal)]
    public void RejectsInvalidNumbersWithColumnName(string value, ColumnType type)
    {
        var input = new Input
        {
            Json = $"[{{\"Count\":{value}}}]",
            ColumnSpecifications = [new ColumnSpecification { Name = "Count", Type = type, Length = 8 }],
        };
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Count"));
    }

    [Test]
    public void RejectsLineBreaksInsideFields()
    {
        var input = new Input
        {
            Json = "[{\"Name\":\"A\\r\\nB\"}]",
            ColumnSpecifications = [new ColumnSpecification { Name = "Name", Length = 4 }],
        };
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("line break"));
    }

    [Test]
    public void RejectsDuplicateColumnNames()
    {
        var input = DefaultInput();
        input.ColumnSpecifications[1].Name = input.ColumnSpecifications[0].Name;
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("duplicate"));
    }

    [Test]
    public void RequiresDateFormatForDateTimeColumns()
    {
        var input = DefaultInput();
        input.ColumnSpecifications[1].DateTimeFormat = string.Empty;
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("DateTimeFormat is required."));
    }

    [TestCase("{}")]
    [TestCase("[]")]
    [TestCase("[1]")]
    public void RejectsJsonThatIsNotANonEmptyArrayOfObjects(string json)
    {
        var input = DefaultInput();
        input.Json = json;
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void RejectsContentAfterTheJsonArray()
    {
        var input = DefaultInput();
        input.Json = "[{\"Name\":\"Veijo\",\"StartDate\":\"2018-05-27T00:00:00\"}] {}";
        var options = new Options { ThrowErrorOnFailure = false };

        var result = FixedWidthFlatFile.CreateFromJson(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Is.Not.Empty);
    }

    [Test]
    public void ThrowsForInvalidJsonWhenConfigured()
    {
        var input = DefaultInput();
        input.Json = "{invalid";

        var exception = Assert.Throws<Newtonsoft.Json.JsonReaderException>(
            () => FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void CancellationIsNotConvertedToFailure()
    {
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => FixedWidthFlatFile.CreateFromJson(DefaultInput(), DefaultOptions(), cancellationSource.Token));
    }

    [Test]
    public void RequiredJsonIsValidated()
    {
        var input = DefaultInput();
        input.Json = null;

        Assert.Throws<ValidationException>(
            () => FixedWidthFlatFile.CreateFromJson(input, DefaultOptions(), CancellationToken.None));
    }
}
