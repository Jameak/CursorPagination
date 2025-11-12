using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.OffsetPagination;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.PartialStrategies;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests;
public class OffsetPaginationTests
{
    private static void AssertStrategyOrdersCorrectly(
        IOffsetPaginationStrategy<SimplePropertyPoco> strategy,
        Func<IQueryable<SimplePropertyPoco>, PaginationDirection, IQueryable<SimplePropertyPoco>> expectedOrderingFunc,
        PaginationDirection direction,
        bool checkHasNextPage,
        int? skipValue)
    {
        var twentyInputElements = TestHelper.InfiniteGenerator().Take(20).ToList();
        var inputQueryable = twentyInputElements.AsQueryable();
        var pageSize = 10;
        // Act
        var (pageData, hasNextPage) = TestHelper.RunOffsetStrategy(
            strategy,
            inputQueryable,
            direction,
            pageSize,
            checkHasNextPage,
            skipValue.HasValue ? new OffsetCursor(skipValue.Value) : null);

        // Assert
        Assert.Equal(pageSize, pageData.Count);

        if (checkHasNextPage)
        {
            Assert.True(hasNextPage);
        }
        else
        {
            Assert.Null(hasNextPage);
        }

        var expectedQueryable = expectedOrderingFunc(inputQueryable, direction);

        if (skipValue.HasValue)
        {
            expectedQueryable = expectedQueryable.Skip(skipValue.Value);
        }

        expectedQueryable = expectedQueryable.Take(10);
        if (direction == PaginationDirection.Backward)
        {
            expectedQueryable = expectedQueryable.Reverse();
        }

        Assert.Equal(expectedQueryable, pageData.Select(e => e.Data));
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, false, null)]
    [InlineData(PaginationDirection.Backward, false, null)]
    [InlineData(PaginationDirection.Forward, true, null)]
    [InlineData(PaginationDirection.Backward, true, null)]
    [InlineData(PaginationDirection.Forward, false, 5)]
    [InlineData(PaginationDirection.Backward, false, 5)]
    [InlineData(PaginationDirection.Forward, true, 5)]
    [InlineData(PaginationDirection.Backward, true, 5)]
    public void OffsetStrategyAllAscending_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new OffsetStrategyAllAscending();
        AssertStrategyOrdersCorrectly(strategy, OffsetStrategyAllAscendingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, false, null)]
    [InlineData(PaginationDirection.Backward, false, null)]
    [InlineData(PaginationDirection.Forward, true, null)]
    [InlineData(PaginationDirection.Backward, true, null)]
    [InlineData(PaginationDirection.Forward, false, 5)]
    [InlineData(PaginationDirection.Backward, false, 5)]
    [InlineData(PaginationDirection.Forward, true, 5)]
    [InlineData(PaginationDirection.Backward, true, 5)]
    public void OffsetStrategyAllDescending_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new OffsetStrategyAllDescending();
        AssertStrategyOrdersCorrectly(strategy, OffsetStrategyAllDescendingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, false, null)]
    [InlineData(PaginationDirection.Backward, false, null)]
    [InlineData(PaginationDirection.Forward, true, null)]
    [InlineData(PaginationDirection.Backward, true, null)]
    [InlineData(PaginationDirection.Forward, false, 5)]
    [InlineData(PaginationDirection.Backward, false, 5)]
    [InlineData(PaginationDirection.Forward, true, 5)]
    [InlineData(PaginationDirection.Backward, true, 5)]
    public void OffsetStrategyMixedOrdering_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new OffsetStrategyMixedOrdering();
        AssertStrategyOrdersCorrectly(strategy, OffsetStrategyMixedOrderingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
    }
}
