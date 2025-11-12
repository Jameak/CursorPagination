using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.InputClasses;

[PrimaryKey(nameof(PrimaryKeyInt))]
public record NullablePropertyPoco
{
    public required int PrimaryKeyInt { get; set; }
    public required int? NullableIntProp { get; set; }
    public required Guid? NullableGuidProp { get; set; }
    public required bool? NullableBoolProp { get; set; }
    public required string? NullableStringProp { get; set; }
}
