using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests.PartialStrategies;

[KeySetPaginationStrategy(typeof(SimplePropertyPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(SimplePropertyPoco.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(-1, nameof(SimplePropertyPoco.StringProp1), PaginationOrdering.Descending)]
[PaginationProperty(2, nameof(SimplePropertyPoco.StringProp2), PaginationOrdering.Descending)]
internal partial class KeySetStrategyMixedOrdering
{
}

internal static class KeySetStrategyMixedOrderingTestHelper
{
    public static IQueryable<SimplePropertyPoco> ApplyExpectedCorrectOrder(IQueryable<SimplePropertyPoco> queryable, PaginationDirection direction)
    {
        if (direction == PaginationDirection.Forward)
        {
            return queryable.OrderByDescending(e => e.StringProp1).ThenBy(e => e.IntProp).ThenByDescending(e => e.StringProp2);
        }

        return queryable.OrderBy(e => e.StringProp1).ThenByDescending(e => e.IntProp).ThenBy(e => e.StringProp2);
    }
}
