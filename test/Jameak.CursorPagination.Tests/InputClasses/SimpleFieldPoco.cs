using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.InputClasses;
[PrimaryKey(nameof(IntField), nameof(StringField))]
public record SimpleFieldPoco
{
    public int IntField;
    public required string StringField;
}
