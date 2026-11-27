using Frends.FixedWidthFlatFile.CreateFromJson.Definitions;

namespace Frends.FixedWidthFlatFile.CreateFromJson.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new()
    {
        Json = "[{\"Name\":\"Veijo\",\"StartDate\":\"2018-05-27T00:00:00\"}]",
        ColumnSpecifications =
        [
            new ColumnSpecification { Name = "Name", Length = 8, Type = ColumnType.String },
            new ColumnSpecification { Name = "StartDate", Length = 8, Type = ColumnType.DateTime, DateTimeFormat = "yyyyMMdd" },
        ],
    };

    protected static Options DefaultOptions() => new();
}
