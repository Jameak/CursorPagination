using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.InputClasses;
[PrimaryKey(nameof(IntProp), nameof(StringProp))]
public record SimplePropertyPoco
{
    public required int IntProp { get; set; }
    public required string StringProp { get; set; }
}
