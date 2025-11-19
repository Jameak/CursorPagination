using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.OffsetPagination;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;
using Jameak.CursorPagination.Tests.DbClasses;
using Jameak.CursorPagination.Tests.InputClasses;

namespace Jameak.CursorPagination.Tests;
public class OffsetPaginatorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFactory;

    public OffsetPaginatorTests(DatabaseFixture databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    [Fact]
    public void FalseCheckNextPage_AndFalseTotalCount_DoesNotComputeNextPageAndTotalCount_Sync()
    {
        var strategy = new SimplePropertyOffsetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData();

        // Act
        var firstPage = OffsetPaginator.ApplyPagination(
            strategy,
            data.AsQueryable(),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.Never,
            computeTotalCount: ComputeTotalCount.Never);

        var pages = new EnumerablePages<SimplePropertyPoco, OffsetCursor>(firstPage);

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
        var strategy = new SimplePropertyOffsetStrategy();
        var data = TestHelper.CreateSimplePropertyPocoData();

        // Act
        var firstPage = await OffsetPaginator.ApplyPaginationAsync(
            strategy,
            data.AsQueryable(),
            asyncMaterializationFunc: (queryable, _) => AsyncDelegates.SyncToList(queryable),
            asyncCountFunc: (queryable, _) => AsyncDelegates.SyncCount(queryable),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.Never,
            computeTotalCount: ComputeTotalCount.Never);

        var pages = new AsyncEnumerablePages<SimplePropertyPoco, OffsetCursor>(firstPage);

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
        var strategy = new SimplePropertyOffsetStrategy();
        var dbContext = _databaseFactory.CreateDbContext();

        // Act
        var firstPage = OffsetPaginator.ApplyPagination(
            strategy,
            TestHelper.TagTestQueryable(dbContext.SimplePropertyTestTable),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: computeTotal,
            paginationDirection: direction);

        var pages = new EnumerablePages<SimplePropertyPoco, OffsetCursor>(firstPage);

        // Assert
        var results = new LinkedList<RowData<SimplePropertyPoco, OffsetCursor>>();
        var nextPageResults = new List<bool>();
        var previousPageResults = new List<bool>();
        foreach (var page in pages)
        {
            Assert.True(page.TotalCount.HasValue);
            Assert.Equal(dbContext.SimplePropertyTestTable.Count(), page.TotalCount.Value);

            Assert.True(page.HasNextPage.HasValue);
            nextPageResults.Add(page.HasNextPage.Value);
            previousPageResults.Add(page.HasPreviousPage());

            TestHelper.CombinePageHelper(results, page.Items, direction);
            Assert.Equal(TestHelper.GetExpectedNextCursor(page.Items, direction), page.NextCursor);
        }

        Assert.True(nextPageResults.Take(nextPageResults.Count - 1).All(b => b), "All pages except last page, should indicate that a next page exists.");
        Assert.False(nextPageResults.Skip(nextPageResults.Count - 1).All(b => b), "Last page should indicate that no next page exists.");
        Assert.True(previousPageResults.Skip(1).All(b => b), "All pages except first page, should indicate that a previous page exists.");
        Assert.False(previousPageResults.First(), "First page should indicate that no previous page exists.");

        // Always forward direction, as the CombinePageHelper combines each page to the forward-direction equivalent.
        Assert.Equal(SimplePropertyOffsetStrategyTestHelper.ApplyExpectedCorrectOrderForwardDirection(dbContext.SimplePropertyTestTable), results.Select(e => e.Data));
        return Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings(direction, computeTotal));
    }

    [Theory]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Forward, ComputeTotalCount.EveryPage)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.Once)]
    [InlineData(PaginationDirection.Backward, ComputeTotalCount.EveryPage)]
    public async Task CheckAsyncPagination(PaginationDirection direction, ComputeTotalCount computeTotal)
    {
        //todo double check this sql.why is it doing minus 1 ?
        var strategy = new SimplePropertyOffsetStrategy();
        var dbContext = _databaseFactory.CreateDbContext();

        // Act
        var firstPage = await OffsetPaginator.ApplyPaginationAsync(
            strategy,
            TestHelper.TagTestQueryable(dbContext.SimplePropertyTestTable),
            asyncMaterializationFunc: (queryable, _) => AsyncDelegates.SyncToList(queryable),
            asyncCountFunc: (queryable, _) => AsyncDelegates.SyncCount(queryable),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: computeTotal,
            paginationDirection: direction);

        var pages = new AsyncEnumerablePages<SimplePropertyPoco, OffsetCursor>(firstPage);

        // Assert
        var results = new LinkedList<RowData<SimplePropertyPoco, OffsetCursor>>();
        var nextPageResults = new List<bool>();
        var previousPageResults = new List<bool>();
        await foreach (var page in pages)
        {
            Assert.True(page.TotalCount.HasValue);
            Assert.Equal(dbContext.SimplePropertyTestTable.Count(), page.TotalCount.Value);

            Assert.True(page.HasNextPage.HasValue);
            nextPageResults.Add(page.HasNextPage.Value);
            previousPageResults.Add(await page.HasPreviousPageAsync());

            TestHelper.CombinePageHelper(results, page.Items, direction);
            Assert.Equal(TestHelper.GetExpectedNextCursor(page.Items, direction), page.NextCursor);
        }

        Assert.True(nextPageResults.Take(nextPageResults.Count - 1).All(b => b), "All pages except last page, should indicate that a next page exists.");
        Assert.False(nextPageResults.Last(), "Last page should indicate that no next page exists.");
        Assert.True(previousPageResults.Skip(1).All(b => b), "All pages except first page, should indicate that a previous page exists.");
        Assert.False(previousPageResults.First(), "First page should indicate that no previous page exists.");

        // Always forward direction, as the CombinePageHelper combines each page to the forward-direction equivalent.
        Assert.Equal(SimplePropertyOffsetStrategyTestHelper.ApplyExpectedCorrectOrderForwardDirection(dbContext.SimplePropertyTestTable), results.Select(e => e.Data));
        await Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings(direction, computeTotal));
    }

    [Fact]
    public void HasPreviousPage_VerifyBehavior()
    {
        var strategy = new SimplePropertyOffsetStrategy();
        var emptyDataSet = new List<SimplePropertyPoco>();

        // First page has no previous page.
        var firstPage = OffsetPaginator.ApplyPagination(
            strategy,
            emptyDataSet.AsQueryable(),
            afterCursor: null,
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: ComputeTotalCount.Never,
            paginationDirection: PaginationDirection.Forward);

        Assert.False(firstPage.HasPreviousPage());

        // First page identified by non-skipping cursor has no previous page.
        var alsoFirstPage = OffsetPaginator.ApplyPagination(
            strategy,
            emptyDataSet.AsQueryable(),
            afterCursor: new OffsetCursor(0),
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: ComputeTotalCount.Never,
            paginationDirection: PaginationDirection.Forward);

        Assert.False(alsoFirstPage.HasPreviousPage());

        // Arbitrary page identified by skipping cursor has previous page even if no data exists in the table, but has no previous data.
        var notFirst = OffsetPaginator.ApplyPagination(
            strategy,
            emptyDataSet.AsQueryable(),
            afterCursor: new OffsetCursor(1),
            pageSize: 3,
            computeNextPage: ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            computeTotalCount: ComputeTotalCount.Never,
            paginationDirection: PaginationDirection.Forward);

        Assert.True(notFirst.HasPreviousPage());
    }
}
