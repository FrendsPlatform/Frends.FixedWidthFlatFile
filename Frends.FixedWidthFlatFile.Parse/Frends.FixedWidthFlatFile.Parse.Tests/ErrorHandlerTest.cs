using Frends.FixedWidthFlatFile.Parse.Definitions;
using NUnit.Framework;
using System;
using System.Threading;

namespace Frends.FixedWidthFlatFile.Parse.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    private static Input DefaultInput()
    {
        // Data row is shorter than the column specifications require, which causes a parsing failure.
        return new Input
        {
            FlatFileContent = "short",
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = new[]
            {
                new ColumnSpecification { Name = "Name", Type = ColumnType.String, Length = 20 },
            },
        };
    }

    private static Options DefaultOptions()
    {
        return new Options { ThrowErrorOnFailure = true, ErrorMessageOnFailure = string.Empty };
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Catch<Exception>(() =>
            FixedWidthFlatFile.Parse(DefaultInput(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = FixedWidthFlatFile.Parse(DefaultInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            FixedWidthFlatFile.Parse(DefaultInput(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Contains.Substring(CustomErrorMessage));
    }
}
