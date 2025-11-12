using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Jameak.CursorPagination.Abstractions.OffsetPagination;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests;
internal class TestHelper
{
    public static IEnumerable<SimplePropertyPoco> InfiniteGenerator()
    {
        var i = 0;
        var j = 0;
        while (true)
        {
            yield return new SimplePropertyPoco
            {
                IntProp = i++,
                StringProp1 = $"{j--}",
                StringProp2 = $"{i++}"
            };
        }
    }

    public static (List<T> pageData, bool? hasNextPage) RunKeySetStrategy<T, TCursor>(
        IKeySetPaginationStrategy<T, TCursor> strategy,
        IQueryable<T> inputQueryable,
        PaginationDirection direction,
        int pageSize,
        bool checkNextPage,
        TCursor? cursor) where TCursor : IKeySetCursor
    {
        var paginationExprs = strategy.BuildPaginationMethods(pageSize, checkNextPage, direction, cursor);
        var rawPage = ApplyAndMaterializeKeySet(paginationExprs, inputQueryable);
        strategy.PostProcessMaterializedResultInPlace(rawPage, pageSize, checkNextPage, direction, out var hasNextPage);
        return (rawPage, hasNextPage);
    }

    public static (List<RowData<T, OffsetCursor>> pageData, bool? hasNextPage) RunOffsetStrategy<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> inputQueryable,
        PaginationDirection direction,
        int pageSize,
        bool checkNextPage,
        OffsetCursor? cursor)
    {
        var paginationExprs = strategy.BuildPaginationMethods(pageSize, checkNextPage, direction, cursor);
        var rawPage = ApplyAndMaterializeOffset(paginationExprs, inputQueryable);
        var processedPage = strategy.PostProcessMaterializedResult(rawPage, pageSize, checkNextPage, direction, cursor, out var hasNextPage);
        return (processedPage.ToList(), hasNextPage);
    }

    public static List<T> ApplyAndMaterializeKeySet<T>(
        (Func<IQueryable<T>, IQueryable<T>> applyWhereExpr,
        Func<IQueryable<T>, IOrderedQueryable<T>> applyOrderExpr,
        Func<IQueryable<T>, IQueryable<T>> applyTake) keysetPaginationExprs,
        IQueryable<T> queryable)
    {
        queryable = keysetPaginationExprs.applyWhereExpr(queryable);
        queryable = keysetPaginationExprs.applyOrderExpr(queryable);
        queryable = keysetPaginationExprs.applyTake(queryable);
        return queryable.ToList();
    }

    public static List<T> ApplyAndMaterializeOffset<T>(
        (Func<IQueryable<T>, IOrderedQueryable<T>> applyOrderExpr,
        Func<IQueryable<T>, IQueryable<T>> applySkip,
        Func<IQueryable<T>, IQueryable<T>> applyTake) offsetPaginationExprs,
        IQueryable<T> queryable)
    {
        queryable = offsetPaginationExprs.applyOrderExpr(queryable);
        queryable = offsetPaginationExprs.applySkip(queryable);
        queryable = offsetPaginationExprs.applyTake(queryable);
        return queryable.ToList();
    }
}
