using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;
using Jameak.CursorPagination.Tests.DbTests;
using Jameak.CursorPagination.Tests.InputClasses;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests;

public class KeySetPaginatorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFactory;

    public KeySetPaginatorTests(DatabaseFixture databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    [Fact]
    public void CheckAssumeNextPageDoesNotChangeSync()
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData().Take(3).ToList();

        // Act
        var firstPageWithNotConstantNext = KeySetPaginator.ApplyPagination(
            strategy,
            data.AsQueryable(),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPage);
        var firstPageWithConstantNext = KeySetPaginator.ApplyPagination(
            strategy,
            data.AsQueryable(),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage);

        // Assert
        Assert.True(firstPageWithNotConstantNext.NextPage().IsEmpty);
        Assert.True(firstPageWithConstantNext.NextPage().IsEmpty);

        data.Add(new SimplePropertyPoco { IntProp = 999, StringProp = "x" });
        Assert.False(firstPageWithNotConstantNext.NextPage().IsEmpty, "When underlying data changes, query with non-constant next page checks to see if new data has arrived.");
        Assert.True(firstPageWithConstantNext.NextPage().IsEmpty, "When underlying data changes, query with constant next page does not re-run the query.");
    }

    [Fact]
    public async Task CheckAssumeNextPageDoesNotChangeAsync()
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData().Take(3).ToList();

        // Act
        var firstPageWithNotConstantNext = await KeySetPaginator.ApplyPaginationAsync(
            strategy,
            data.AsQueryable(),
            asyncMaterializationFunc: (queryable, token) => Task.FromResult(queryable.ToList()),
            asyncCountFunc: null,
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPage);
        var firstPageWithConstantNext = await KeySetPaginator.ApplyPaginationAsync(
            strategy,
            data.AsQueryable(),
            asyncMaterializationFunc: (queryable, token) => Task.FromResult(queryable.ToList()),
            asyncCountFunc: null,
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage);

        // Assert
        Assert.True((await firstPageWithNotConstantNext.NextPageAsync()).IsEmpty);
        Assert.True((await firstPageWithConstantNext.NextPageAsync()).IsEmpty);

        data.Add(new SimplePropertyPoco { IntProp = 999, StringProp = "x" });
        Assert.False((await firstPageWithNotConstantNext.NextPageAsync()).IsEmpty, "When underlying data changes, query with non-constant next page checks to see if new data has arrived.");
        Assert.True((await firstPageWithConstantNext.NextPageAsync()).IsEmpty, "When underlying data changes, query with constant next page does not re-run the query.");
    }

    [Fact]
    public async Task AsyncWithTotalCountTrueMustEnforceNonNullCountMethod()
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData();

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => KeySetPaginator.ApplyPaginationAsync(
            strategy,
            data.AsQueryable(),
            asyncMaterializationFunc: (queryable, token) => Task.FromResult(queryable.ToList()),
            asyncCountFunc: null,
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.Never,
            computeTotalCount: ComputeTotalCount.Once));
    }

    [Fact]
    public void FalseCheckNextPage_AndFalseTotalCount_DoesNotComputeNextPageAndTotalCount_Sync()
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData();

        // Act
        var firstPage = KeySetPaginator.ApplyPagination(
            strategy,
            data.AsQueryable(),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.Never,
            computeTotalCount: ComputeTotalCount.Never);

        var pages = new EnumerablePages<SimplePropertyPoco, SimplePropertyKeySetStrategy.Cursor>(firstPage);

        // Assert
        foreach (var page in pages)
        {
            Assert.False(page.TotalCount.HasValue);
            Assert.False(page.HasNextPage.HasValue);
        }
    }

    [Fact]
    public async Task FalseCheckNextPage_AndFalseTotalCount_DoesNotComputeNextPageAndTotalCount_Async()
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData();

        // Act
        var firstPage = await KeySetPaginator.ApplyPaginationAsync(
            strategy,
            data.AsQueryable(),
            asyncMaterializationFunc: (queryable, token) => Task.FromResult(queryable.ToList()),
            asyncCountFunc: null,
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.Never,
            computeTotalCount: ComputeTotalCount.Never);

        var pages = new AsyncEnumerablePages<SimplePropertyPoco, SimplePropertyKeySetStrategy.Cursor>(firstPage);

        // Assert
        await foreach (var page in pages)
        {
            Assert.False(page.TotalCount.HasValue);
            Assert.False(page.HasNextPage.HasValue);
        }
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.EveryPage)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.EveryPage)]
    public Task CheckSyncPagination(PaginationDirection direction, ComputeTotalCount computeTotal)
    {
        return InternalCheckSyncPagination<SimplePropertyKeySetStrategy, SimplePropertyPoco, SimplePropertyKeySetStrategy.Cursor>(
            direction,
            computeTotal,
            (dbContext => dbContext.SimplePropertyTestTable),
            (dbContext => SimplePropertyKeySetStrategyTestHelper.ApplyExpectedCorrectOrderForwardDirection(dbContext.SimplePropertyTestTable)));
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.EveryPage)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.EveryPage)]
    public Task CheckSyncPagination_WithEfModelUsingFields(PaginationDirection direction, ComputeTotalCount computeTotal)
    {
        return InternalCheckSyncPagination<SimpleFieldKeySetStrategy, SimpleFieldPoco, SimpleFieldKeySetStrategy.Cursor>(
            direction,
            computeTotal,
            (dbContext => dbContext.SimpleFieldTestTable),
            (dbContext => SimpleFieldKeySetStrategyTestHelper.ApplyExpectedCorrectOrderForwardDirection(dbContext.SimpleFieldTestTable)));
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.EveryPage)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.EveryPage)]
    public async Task CheckAsyncPagination(PaginationDirection direction, ComputeTotalCount computeTotal)
    {
        var strategy = new SimplePropertyKeySetStrategy();
        var dbContext = _databaseFactory.CreateDbContext();

        // Act
        var firstPage = await KeySetPaginator.ApplyPaginationAsync(
            strategy,
            TestHelper.TaggedTestTable(dbContext),
            asyncMaterializationFunc: (queryable, token) => Task.FromResult(queryable.ToList()),
            asyncCountFunc: (queryable, token) => Task.FromResult(queryable.Count()),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: computeTotal,
            paginationDirection: direction);

        var pages = new AsyncEnumerablePages<SimplePropertyPoco, SimplePropertyKeySetStrategy.Cursor>(firstPage);

        // Assert
        var results = new LinkedList<RowData<SimplePropertyPoco, SimplePropertyKeySetStrategy.Cursor>>();
        var nextPageResults = new List<bool>();
        await foreach (var page in pages)
        {
            Assert.True(page.TotalCount.HasValue);
            Assert.Equal(dbContext.SimplePropertyTestTable.Count(), page.TotalCount.Value);

            Assert.True(page.HasNextPage.HasValue);
            nextPageResults.Add(page.HasNextPage.Value);

            TestHelper.CombinePageHelper(results, page.Items, direction);
            Assert.Equal(TestHelper.GetExpectedNextCursor(page.Items, direction), page.NextCursor);
        }

        Assert.True(nextPageResults.Take(nextPageResults.Count - 1).All(b => b), "All pages except last page, should indicate that a next page exists.");
        Assert.False(nextPageResults.Skip(nextPageResults.Count - 1).All(b => b), "Last page should indicate that no next page exists.");

        // Always forward direction, as the CombinePageHelper combines each page to the forward-direction equivalent.
        Assert.Equal(SimplePropertyKeySetStrategyTestHelper.ApplyExpectedCorrectOrderForwardDirection(dbContext.SimplePropertyTestTable), results.Select(e => e.Data));
        await Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings(direction, computeTotal));
    }

    private Task InternalCheckSyncPagination<TStrategy, T, TCursor>(
        PaginationDirection direction,
        ComputeTotalCount computeTotal,
        Func<TestDbContext, DbSet<T>> getDbTable,
        Func<TestDbContext, IEnumerable<T>> expectedOrder)
        where TStrategy : IKeySetPaginationStrategy<T, TCursor>, new()
        where TCursor : class, IKeySetCursor
        where T : class
    {
        var strategy = new TStrategy();
        var dbContext = _databaseFactory.CreateDbContext();

        // Act
        var firstPage = KeySetPaginator.ApplyPagination(
            strategy,
            TestHelper.TagTestQueryable(getDbTable(dbContext)),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: computeTotal,
            paginationDirection: direction);

        var pages = new EnumerablePages<T, TCursor>(firstPage);

        // Assert
        var results = new LinkedList<RowData<T, TCursor>>();
        var nextPageResults = new List<bool>();
        foreach (var page in pages)
        {
            Assert.True(page.TotalCount.HasValue);
            Assert.Equal(dbContext.SimplePropertyTestTable.Count(), page.TotalCount.Value);

            Assert.True(page.HasNextPage.HasValue);
            nextPageResults.Add(page.HasNextPage.Value);

            TestHelper.CombinePageHelper(results, page.Items, direction);
            Assert.Equal(TestHelper.GetExpectedNextCursor(page.Items, direction), page.NextCursor);
        }

        Assert.True(nextPageResults.Take(nextPageResults.Count - 1).All(b => b), "All pages except last page, should indicate that a next page exists.");
        Assert.False(nextPageResults.Skip(nextPageResults.Count - 1).All(b => b), "Last page should indicate that no next page exists.");

        // Always forward direction, as the CombinePageHelper combines each page to the forward-direction equivalent.
        Assert.Equal(expectedOrder(dbContext), results.Select(e => e.Data));
        return Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings(direction, computeTotal));
    }
}
