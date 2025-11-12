using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[OffsetPaginationStrategy(typeof(SimplePropertyPoco))]
[PaginationProperty(0, nameof(SimplePropertyPoco.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(SimplePropertyPoco.StringProp), PaginationOrdering.Descending)]
internal partial class SimplePropertyOffsetStrategy
{
}

internal static class SimplePropertyOffsetStrategyTestHelper
{
    public static IQueryable<SimplePropertyPoco> ApplyExpectedCorrectOrderForwardDirection(IQueryable<SimplePropertyPoco> queryable)
    {
        return queryable.OrderBy(e => e.IntProp).ThenByDescending(e => e.StringProp);
    }
}
