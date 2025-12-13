using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.InputClasses;
[PrimaryKey(nameof(IntProp))]
public record PocoWithNestedProperty
{
    public required int IntProp { get; set; }
    public required NestedData NestedData { get; set; }
}

[PrimaryKey(nameof(IntProp))]
public record NestedData
{
    public required int IntProp { get; set; }
    public required string StringProp { get; set; }
}
