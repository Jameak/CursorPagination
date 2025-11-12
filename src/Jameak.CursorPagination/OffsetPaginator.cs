using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.OffsetPagination;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;

namespace Jameak.CursorPagination;

/// <summary>
/// Pagination methods for Offset pagination
/// </summary>
public static class OffsetPaginator
{
    #region Sync
    private static PageResult<T, OffsetCursor> InternalApplyPagination<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        OffsetCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage,
        PaginationDirection paginationDirection,
        int? totalCount,
        ComputeTotalCount computeTotalCount)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(queryable);
        InternalPaginatorHelper.ThrowIfEnumNotDefined(computeNextPage);
        InternalPaginatorHelper.ThrowIfEnumNotDefined(computeTotalCount);

        if (InternalPaginatorHelper.ShouldComputeTotalCount(totalCount.HasValue, computeTotalCount))
        {
            totalCount = queryable.Count();
        }

        var paginatedQueryable = strategy.ApplyPagination(queryable, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, afterCursor);
        var materialized = paginatedQueryable.ToList();

        var postProcessed = strategy.PostProcessMaterializedResult(materialized, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, afterCursor, out var hasNextPage)
            .ToList();

        var nextCursorElement = paginationDirection == PaginationDirection.Forward
            ? postProcessed.LastOrDefault()
            : postProcessed.FirstOrDefault();

        NextPage<T, OffsetCursor> NextPageGenerator(OffsetCursor nextCursor)
        {
            return () => InternalApplyPagination(
                strategy: strategy,
                queryable: queryable,
                afterCursor: nextCursor,
                pageSize: pageSize,
                computeNextPage: computeNextPage,
                paginationDirection: paginationDirection,
                totalCount: totalCount,
                computeTotalCount: computeTotalCount);
        }

        var nextPageFunc = InternalPaginatorHelper.DetermineNextPageFunc(NextPageGenerator, elem => elem.Cursor, nextCursorElement, totalCount, hasNextPage, computeNextPage);

        return new PageResult<T, OffsetCursor>(postProcessed, hasNextPage, totalCount, nextPageFunc, nextCursorElement?.Cursor);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="afterCursor">The cursor to use as the starting point for the pagination. To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.
    /// </remarks>
    public static PageResult<T, OffsetCursor> ApplyPagination<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        OffsetCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never)
    {
        return InternalApplyPagination(
            strategy: strategy,
            queryable: queryable,
            afterCursor: afterCursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="afterCursorString">The opaque cursor string to use as the starting point for the pagination. To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.
    /// </remarks>
    public static PageResult<T, OffsetCursor> ApplyPagination<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        string? afterCursorString,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never)
    {
        var cursor = afterCursorString == null ? null : OffsetCursor.CursorFromString(afterCursorString);
        return InternalApplyPagination(
            strategy: strategy,
            queryable: queryable,
            afterCursor: cursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="pageNumber">The 1-indexed page to retrieve.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">Throw if the page-number is negative or zero.</exception>
    public static PageResult<T, OffsetCursor> ApplyPagination<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        int pageNumber,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("Page number must be greater than or equal to 1.", nameof(pageNumber));
        }

        OffsetCursor cursor;
        checked
        {
            cursor = new OffsetCursor((pageNumber - 1) * pageSize);
        }

        return InternalApplyPagination(
            strategy,
            queryable,
            cursor,
            pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount);
    }
    #endregion

    #region Async
    private static async Task<PageResultAsync<T, OffsetCursor>> InternalApplyPaginationAsync<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        OffsetCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage,
        PaginationDirection paginationDirection,
        int? totalCount,
        ComputeTotalCount computeTotalCount,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(queryable);
        ArgumentNullException.ThrowIfNull(asyncMaterializationFunc);
        InternalPaginatorHelper.ThrowIfEnumNotDefined(computeNextPage);
        InternalPaginatorHelper.ThrowIfEnumNotDefined(computeTotalCount);

        if (InternalPaginatorHelper.ShouldComputeTotalCount(totalCount.HasValue, computeTotalCount))
        {
            if (asyncCountFunc == null)
            {
                throw new ArgumentException($"Argument '{nameof(asyncCountFunc)}' must be non-null when total count computation is enabled", nameof(asyncCountFunc));
            }

            totalCount = await asyncCountFunc(queryable, cancellationToken);
        }

        var paginatedQueryable = strategy.ApplyPagination(queryable, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, afterCursor);
        var materialized = await asyncMaterializationFunc(paginatedQueryable, cancellationToken);

        var postProcessed = strategy.PostProcessMaterializedResult(materialized, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, afterCursor, out var hasNextPage)
            .ToList();

        var nextCursorElement = paginationDirection == PaginationDirection.Forward
            ? postProcessed.LastOrDefault()
            : postProcessed.FirstOrDefault();

        NextPageAsync<T, OffsetCursor> NextPageAsyncGenerator(OffsetCursor nextCursor)
        {
            return (cancellationToken) => InternalApplyPaginationAsync(
                strategy: strategy,
                queryable: queryable,
                asyncMaterializationFunc: asyncMaterializationFunc,
                asyncCountFunc: asyncCountFunc,
                afterCursor: nextCursor,
                pageSize: pageSize,
                computeNextPage: computeNextPage,
                paginationDirection: paginationDirection,
                totalCount: totalCount,
                computeTotalCount: computeTotalCount,
                cancellationToken: cancellationToken);
        }

        var nextPageAsyncFunc = InternalPaginatorHelper.DetermineNextPageAsyncFunc(
            NextPageAsyncGenerator,
            elem => elem.Cursor,
            nextCursorElement,
            totalCount,
            hasNextPage,
            computeNextPage);

        return new PageResultAsync<T, OffsetCursor>(
            postProcessed,
            hasNextPage,
            totalCount,
            nextPageAsyncFunc,
            nextCursorElement?.Cursor);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="asyncMaterializationFunc">The function to use to perform async materialization of the <see cref="IQueryable{T}"/></param>
    /// <param name="asyncCountFunc">The function to use to perform async counting of the <see cref="IQueryable{T}"/></param>
    /// <param name="afterCursor">The cursor to use as the starting point for the pagination. To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for async materialization.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// <para>Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.</para>
    /// <para>
    /// If using EFCore for async operations you can create the required async funcs like so:
    /// <code>
    /// asyncMaterializationFunc: (queryable, cancellationToken) => queryable.ToListAsync(cancellationToken)
    /// asyncCountFunc: (queryable, cancellationToken) => queryable.CountAsync(cancellationToken)
    /// </code>
    /// </para> 
    /// </remarks>
    public static Task<PageResultAsync<T, OffsetCursor>> ApplyPaginationAsync<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        OffsetCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never,
        CancellationToken cancellationToken = default)
    {
        return InternalApplyPaginationAsync(
            strategy: strategy,
            queryable: queryable,
            asyncMaterializationFunc: asyncMaterializationFunc,
            asyncCountFunc: asyncCountFunc,
            afterCursor: afterCursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="asyncMaterializationFunc">The function to use to perform async materialization of the <see cref="IQueryable{T}"/></param>
    /// <param name="asyncCountFunc">The function to use to perform async counting of the <see cref="IQueryable{T}"/></param>
    /// <param name="afterCursorString">The opaque cursor string to use as the starting point for the pagination. To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for async materialization.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// <para>Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.</para>
    /// <para>
    /// If using EFCore for async operations you can create the required async funcs like so:
    /// <code>
    /// asyncMaterializationFunc: (queryable, cancellationToken) => queryable.ToListAsync(cancellationToken)
    /// asyncCountFunc: (queryable, cancellationToken) => queryable.CountAsync(cancellationToken)
    /// </code>
    /// </para> 
    /// </remarks>
    public static Task<PageResultAsync<T, OffsetCursor>> ApplyPaginationAsync<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        string? afterCursorString,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never,
        CancellationToken cancellationToken = default)
    {
        var cursor = afterCursorString == null ? null : OffsetCursor.CursorFromString(afterCursorString);
        return InternalApplyPaginationAsync(
            strategy: strategy,
            queryable: queryable,
            asyncMaterializationFunc: asyncMaterializationFunc,
            asyncCountFunc: asyncCountFunc,
            afterCursor: cursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Paginates and materializes the queryable using Offset pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="strategy">The Offset pagination strategy to use for pagination.</param>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> to paginate</param>
    /// <param name="asyncMaterializationFunc">The function to use to perform async materialization of the <see cref="IQueryable{T}"/></param>
    /// <param name="asyncCountFunc">The function to use to perform async counting of the <see cref="IQueryable{T}"/></param>
    /// <param name="pageNumber">The 1-indexed page to retrieve.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="computeNextPage">Controls whether each page should check if a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="computeTotalCount">Controls whether to compute the total number of elements.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for async materialization.</param>
    /// <returns>The page result with items after/before the given cursor as determined by the <paramref name="paginationDirection"/> argument.</returns>
    /// <remarks>
    /// <para>Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.</para>
    /// <para>
    /// If using EFCore for async operations you can create the required async funcs like so:
    /// <code>
    /// asyncMaterializationFunc: (queryable, cancellationToken) => queryable.ToListAsync(cancellationToken)
    /// asyncCountFunc: (queryable, cancellationToken) => queryable.CountAsync(cancellationToken)
    /// </code>
    /// </para> 
    /// </remarks>
    /// <exception cref="ArgumentException">Throw if the page-number is negative or zero.</exception>
    public static Task<PageResultAsync<T, OffsetCursor>> ApplyPaginationAsync<T>(
        IOffsetPaginationStrategy<T> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        int pageNumber,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("Page number must be greater than or equal to 1.", nameof(pageNumber));
        }

        OffsetCursor cursor;
        checked
        {
            cursor = new OffsetCursor((pageNumber - 1) * pageSize);
        }
        return InternalApplyPaginationAsync(
            strategy: strategy,
            queryable: queryable,
            asyncMaterializationFunc: asyncMaterializationFunc,
            asyncCountFunc: asyncCountFunc,
            afterCursor: cursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            paginationDirection: paginationDirection,
            totalCount: null,
            computeTotalCount: computeTotalCount,
            cancellationToken: cancellationToken);
    }
    #endregion
}
