using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.PartialStrategies;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests;
public class KeySetPaginationTests
{
    private static void AssertStrategyOrdersCorrectly<TCursor>(
        IKeySetPaginationStrategy<SimplePropertyPoco, TCursor> strategy,
        Func<IQueryable<SimplePropertyPoco>, PaginationDirection, IQueryable<SimplePropertyPoco>> expectedOrderingFunc,
        PaginationDirection direction,
        bool checkHasNextPage,
        int? skipValue) where TCursor : class, IKeySetCursor
    {
        var twentyInputElements = TestHelper.InfiniteGenerator().Take(20).ToList();
        var inputQueryable = twentyInputElements.AsQueryable();
        var pageSize = 10;
        var cursorRow = skipValue.HasValue ? expectedOrderingFunc(inputQueryable, direction).Skip(skipValue.Value - 1).First() : null;
        var cursor = cursorRow == null ? null : strategy.CreateCursor(cursorRow);
        // Act
        var (pageData, hasNextPage) = TestHelper.RunKeySetStrategy(strategy, inputQueryable, direction, pageSize, checkHasNextPage, cursor);

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

        Assert.Equal(expectedQueryable, pageData);
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
    public void KeySetStrategyAllAscending_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new KeySetStrategyAllAscending();
        AssertStrategyOrdersCorrectly(strategy, KeySetStrategyAllAscendingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
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
    public void KeySetStrategyAllDescending_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new KeySetStrategyAllDescending();
        AssertStrategyOrdersCorrectly(strategy, KeySetStrategyAllDescendingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
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
    public void KeySetStrategyMixedOrdering_OrdersCorrectly(PaginationDirection direction, bool checkHasNextPage, int? skipValue)
    {
        var strategy = new KeySetStrategyMixedOrdering();
        AssertStrategyOrdersCorrectly(strategy, KeySetStrategyMixedOrderingTestHelper.ApplyExpectedCorrectOrder, direction, checkHasNextPage, skipValue);
    }
}
