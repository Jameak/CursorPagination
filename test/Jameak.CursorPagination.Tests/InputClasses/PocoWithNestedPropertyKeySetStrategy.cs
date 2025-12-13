using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(PocoWithNestedProperty), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@PocoWithNestedProperty.NestedData.StringProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(PocoWithNestedProperty.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(2, nameof(@PocoWithNestedProperty.NestedData.IntProp), PaginationOrdering.Ascending)]
internal partial class PocoWithNestedPropertyKeySetStrategy
{
}

internal static class PocoWithNestedPropertyKeySetStrategyTestHelper
{
    public static IQueryable<PocoWithNestedProperty> ApplyExpectedCorrectOrderForwardDirection(IQueryable<PocoWithNestedProperty> queryable)
    {
        return queryable.OrderBy(e => e.NestedData.StringProp).ThenBy(e => e.IntProp).ThenBy(e => e.NestedData.IntProp);
    }
}
