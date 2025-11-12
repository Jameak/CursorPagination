using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.InputClasses;
[PrimaryKey(nameof(PrimaryKeyInt))]
[Index(nameof(ComputedIntProp))]
public record NullablePropertyWithDbComputedColumnPoco
{
    public required int PrimaryKeyInt { get; set; }
    public required int? NullableIntProp { get; set; }
    public int ComputedIntProp { get; set; }
}
