using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(SimpleFieldPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(SimpleFieldPoco.IntField), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(SimpleFieldPoco.StringField), PaginationOrdering.Descending)]
internal partial class SimpleFieldKeySetStrategy
{
}

internal static class SimpleFieldKeySetStrategyTestHelper
{
    public static IQueryable<SimpleFieldPoco> ApplyExpectedCorrectOrderForwardDirection(IQueryable<SimpleFieldPoco> queryable)
    {
        return queryable.OrderBy(e => e.IntField).ThenByDescending(e => e.StringField);
    }
}
