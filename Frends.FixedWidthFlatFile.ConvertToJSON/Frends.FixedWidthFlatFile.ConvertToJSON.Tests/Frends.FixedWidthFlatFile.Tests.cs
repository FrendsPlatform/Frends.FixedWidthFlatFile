using System;
using System.Linq;
using System.Threading;
using Frends.FixedWidthFlatFile.ConvertToJSON.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.FixedWidthFlatFile.ConvertToJSON.Tests;

[TestFixture]
public class Tests
{
    private const string Content = "Name  Street    Start     Amount\n" +
                                   "Veijo FrendsStr 20180527  12.5\n" +
                                   "Hodor HodorsStr 20180101  3.25\n";

    private static ColumnSpecification[] Specs() =>
    [
        new() { Name = "Name", Type = ColumnType.String, Length = 6 },
        new() { Name = "Street", Type = ColumnType.String, Length = 10 },
        new() { Name = "Start", Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 10 },
        new() { Name = "Amount", Type = ColumnType.Decimal, Length = 4 }
    ];

    private static Input DefaultInput(string content = Content) => new() { FlatFileContent = content };

    private static Options DefaultOptions(HeaderRowType header = HeaderRowType.FixedWidth) => new()
    {
        HeaderRow = header,
        ColumnSpecifications = Specs(),
    };

    private static JArray Run(Input input, Options options)
    {
        var result = FixedWidthFlatFile.ConvertToJSON(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        return JArray.Parse(result.Data);
    }

    [Test]
    public void ConvertsFixedWidthHeaderContent()
    {
        var json = Run(DefaultInput(), DefaultOptions());
        Assert.That(json.Count, Is.EqualTo(2));
        Assert.That((string)json[0]["Name"], Is.EqualTo("Veijo"));
        Assert.That((string)json[0]["Street"], Is.EqualTo("FrendsStr"));
        Assert.That((decimal)json[0]["Amount"], Is.EqualTo(12.5m));
        Assert.That((decimal)json[1]["Amount"], Is.EqualTo(3.25m));
        Assert.That(((DateTime)json[1]["Start"]).Year, Is.EqualTo(2018));
    }

    [Test]
    public void ParsesValuesWithGivenCultureAndWritesInvariantJson()
    {
        var content = "Name  Street    Start     Amount\n" +
                      "Veijo FrendsStr 20180527  3,25\n";
        var options = DefaultOptions();
        options.Culture = "fi-FI";
        var result = FixedWidthFlatFile.ConvertToJSON(DefaultInput(content), options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
        var amount = JArray.Parse(result.Data)[0]["Amount"];
        Assert.That(amount.Type, Is.EqualTo(JTokenType.Float));
        Assert.That((decimal)amount, Is.EqualTo(3.25m));
    }

    [Test]
    public void ParsesMonthNamesWithGivenCulture()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            Culture = "fi-FI",
            ColumnSpecifications =
            [
                new ColumnSpecification { Name = "Start", Type = ColumnType.DateTime, DateTimeFormat = "d. MMMM yyyy", Length = 19 }
            ],
        };
        var json = Run(DefaultInput("27. toukokuuta 2018"), options);
        Assert.That(((DateTime)json[0]["Start"]).Month, Is.EqualTo(5));
    }

    [Test]
    public void ConvertsWithoutHeader()
    {
        var content = Content.Split('\n').Skip(1).Aggregate((a, b) => a + "\n" + b);
        var json = Run(DefaultInput(content), DefaultOptions(HeaderRowType.None));
        Assert.That(json.Count, Is.EqualTo(2));
        Assert.That((string)json[1]["Name"], Is.EqualTo("Hodor"));
    }

    [Test]
    public void ConvertsDelimitedHeaderAndUsesItAsNames()
    {
        var options = DefaultOptions(HeaderRowType.Delimited);
        options.HeaderDelimiter = ";";
        foreach (var spec in options.ColumnSpecifications) spec.Name = string.Empty;
        var json = Run(DefaultInput("A;B;C;D\nVeijo FrendsStr 20180527  12.5"), options);
        Assert.That((string)json[0]["A"], Is.EqualTo("Veijo"));
        Assert.That(json[0]["D"], Is.Not.Null);
    }

    [Test]
    public void ConvertToJSON_SkipRowsFromTop_SkipsFirstDataRow()
    {
        var options = DefaultOptions();
        options.SkipRows = true;
        options.SkipRowsFromTop = 1;
        var json = Run(DefaultInput(), options);
        Assert.That(json.Count, Is.EqualTo(1));
        Assert.That((string)json[0]["Name"], Is.EqualTo("Hodor"));
    }

    [Test]
    public void ConvertToJSON_SkipRowsFromBottom_SkipsLastDataRow()
    {
        var options = DefaultOptions();
        options.SkipRows = true;
        options.SkipRowsFromBottom = 1;
        var json = Run(DefaultInput(), options);
        Assert.That(json.Count, Is.EqualTo(1));
        Assert.That((string)json[0]["Name"], Is.EqualTo("Veijo"));
    }

    [Test]
    public void EmptyValueWithoutColumnNameDoesNotThrow()
    {
        var options = DefaultOptions(HeaderRowType.None);
        options.ColumnSpecifications =
        [
            new ColumnSpecification { Length = 6 },
            new ColumnSpecification { Length = 10 }
        ];
        var json = Run(DefaultInput("Veijo           "), options);
        Assert.That((string)json[0]["Field_1"], Is.EqualTo("Veijo"));
        Assert.That(json[0]["Field_2"].Type, Is.EqualTo(JTokenType.String));
    }

    [Test]
    public void MismatchingSpecificationFails()
    {
        var options = DefaultOptions(HeaderRowType.None);
        options.ThrowErrorOnFailure = false;
        var result = FixedWidthFlatFile.ConvertToJSON(DefaultInput("short"), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Contains.Substring("column specifications"));
    }

    private static Result RunExpectingFailure(string content, ColumnSpecification spec)
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ThrowErrorOnFailure = false,
            ColumnSpecifications = [spec],
        };
        var result = FixedWidthFlatFile.ConvertToJSON(DefaultInput(content), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        return result;
    }

    [Test]
    public void ConvertToJSON_MissingColumnLength_Fails()
    {
        var result = RunExpectingFailure("42", new ColumnSpecification { Name = "A" });
        Assert.That(result.Error.Message, Contains.Substring("greater than zero"));
    }

    [Test]
    public void ConvertToJSON_RowLongerThanSpecifications_Fails()
    {
        var result = RunExpectingFailure("12.59", new ColumnSpecification { Name = "A", Type = ColumnType.Decimal, Length = 4 });
        Assert.That(result.Error.Message, Contains.Substring("longer than"));
    }

    [Test]
    public void ConvertToJSON_RowWithTrailingWhitespace_Succeeds()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "A", Length = 4 }],
        };
        var json = Run(DefaultInput("abcd   "), options);
        Assert.That((string)json[0]["A"], Is.EqualTo("abcd"));
    }

    [Test]
    public void ConvertToJSON_NonFiniteDouble_Fails()
    {
        var result = RunExpectingFailure("1e309", new ColumnSpecification { Name = "A", Type = ColumnType.Double, Length = 5 });
        Assert.That(result.Error.Message, Contains.Substring("not a finite number"));
    }

    [Test]
    public void ConvertToJSON_FiniteDouble_WritesJsonNumber()
    {
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ColumnSpecifications = [new ColumnSpecification { Name = "A", Type = ColumnType.Double, Length = 5 }],
        };
        var json = Run(DefaultInput("1.5e3"), options);
        Assert.That(json[0]["A"].Type, Is.EqualTo(JTokenType.Float));
        Assert.That((double)json[0]["A"], Is.EqualTo(1500d));
    }

    [Test]
    public void ConvertToJSON_CanceledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var options = new Options
        {
            HeaderRow = HeaderRowType.None,
            ThrowErrorOnFailure = false,
            ColumnSpecifications = [new ColumnSpecification { Name = "A", Length = 4 }],
        };
        Action act = () => FixedWidthFlatFile.ConvertToJSON(DefaultInput("abcd"), options, cts.Token);
        Assert.Throws<OperationCanceledException>(act);
    }

    [Test]
    public void ConvertToJSON_HostJsonDefaultSettings_DoNotAffectOutput()
    {
        var previous = JsonConvert.DefaultSettings;
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("fi-FI"),
            DateFormatString = "MMMM yyyy",
            Formatting = Formatting.Indented,
        };
        try
        {
            var options = new Options
            {
                HeaderRow = HeaderRowType.None,
                ColumnSpecifications = [new ColumnSpecification { Name = "D", Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd", Length = 8 }],
            };
            var result = FixedWidthFlatFile.ConvertToJSON(DefaultInput("20180527"), options, CancellationToken.None);
            Assert.That(result.Data, Is.EqualTo("[{\"D\":\"2018-05-27T00:00:00\"}]"));
        }
        finally
        {
            JsonConvert.DefaultSettings = previous;
        }
    }
}
