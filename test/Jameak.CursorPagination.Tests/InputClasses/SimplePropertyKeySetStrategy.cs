using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(SimplePropertyPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(SimplePropertyPoco.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(SimplePropertyPoco.StringProp), PaginationOrdering.Descending)]
internal partial class SimplePropertyKeySetStrategy
{
}

internal static class SimplePropertyKeySetStrategyTestHelper
{
    public static IQueryable<SimplePropertyPoco> ApplyExpectedCorrectOrderForwardDirection(IQueryable<SimplePropertyPoco> queryable)
    {
        return queryable.OrderBy(e => e.IntProp).ThenByDescending(e => e.StringProp);
    }
}
