using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;

namespace Jameak.CursorPagination;

/// <summary>
/// Pagination methods for KeySet pagination
/// </summary>
public static class KeySetPaginator
{
    #region Sync
    private static PageResult<T, TCursor> InternalApplyPagination<T, TCursor>(
        IKeySetPaginationStrategy<T, TCursor> strategy,
        IQueryable<T> queryable,
        TCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage,
        PaginationDirection paginationDirection,
        int? totalCount,
        ComputeTotalCount computeTotalCount)
        where TCursor : IKeySetCursor
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

        strategy.PostProcessMaterializedResultInPlace(materialized, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, out var hasNextPage);
        var nextCursorElement = paginationDirection == PaginationDirection.Forward
            ? materialized.LastOrDefault()
            : materialized.FirstOrDefault();

        NextPage<T, TCursor> NextPageGenerator(TCursor nextCursor)
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

        var nextPageFunc = InternalPaginatorHelper.DetermineNextPageFunc(
            NextPageGenerator,
            strategy.CreateCursor,
            nextCursorElement,
            totalCount,
            hasNextPage,
            computeNextPage);

        return new PageResult<T, TCursor>(
            materialized.Select(e => new RowData<T, TCursor>(e, strategy.CreateCursor(e))).ToList(),
            hasNextPage,
            totalCount,
            nextPageFunc,
            nextCursorElement == null ? default : strategy.CreateCursor(nextCursorElement));
    }

    /// <summary>
    /// Paginates and materializes the queryable using KeySet pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <typeparam name="TCursor">The KeySet cursor type</typeparam>
    /// <param name="strategy">The KeySet pagination strategy to use for pagination.</param>
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
    public static PageResult<T, TCursor> ApplyPagination<T, TCursor>(
        IKeySetPaginationStrategy<T, TCursor> strategy,
        IQueryable<T> queryable,
        TCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never)
        where TCursor : IKeySetCursor
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
    /// Paginates and materializes the queryable using KeySet pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <typeparam name="TCursor">The KeySet cursor type</typeparam>
    /// <typeparam name="TStrategy">The KeySet pagination strategy type, which must enable serialization support by implementing the <see cref="IKeySetCursorSerializer{TCursor}"/> interface.</typeparam>
    /// <param name="strategy">The KeySet pagination strategy to use for pagination.</param>
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
    public static PageResult<T, TCursor> ApplyPagination<T, TCursor, TStrategy>(
        TStrategy strategy,
        IQueryable<T> queryable,
        string? afterCursorString,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never)
        where TCursor : class, IKeySetCursor
        where TStrategy : IKeySetPaginationStrategy<T, TCursor>, IKeySetCursorSerializer<TCursor>
    {
        var cursor = afterCursorString == null ? null : strategy.CursorFromString(afterCursorString);
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
    #endregion

    #region Async
    private static async Task<PageResultAsync<T, TCursor>> InternalApplyPaginationAsync<T, TCursor>(
        IKeySetPaginationStrategy<T, TCursor> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        TCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage,
        PaginationDirection paginationDirection,
        int? totalCount,
        ComputeTotalCount computeTotalCount,
        CancellationToken cancellationToken)
        where TCursor : IKeySetCursor
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

        strategy.PostProcessMaterializedResultInPlace(materialized, pageSize, computeNextPage != ComputeNextPage.Never, paginationDirection, out var hasNextPage);

        var nextCursorElement = paginationDirection == PaginationDirection.Forward
            ? materialized.LastOrDefault()
            : materialized.FirstOrDefault();

        NextPageAsync<T, TCursor> NextPageAsyncGenerator(TCursor nextCursor)
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
            strategy.CreateCursor,
            nextCursorElement,
            totalCount,
            hasNextPage,
            computeNextPage);

        return new PageResultAsync<T, TCursor>(
            materialized.Select(e => new RowData<T, TCursor>(e, strategy.CreateCursor(e))).ToList(),
            hasNextPage,
            totalCount,
            nextPageAsyncFunc,
            nextCursorElement == null ? default : strategy.CreateCursor(nextCursorElement));
    }

    /// <summary>
    /// Paginates and materializes the queryable using KeySet pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <typeparam name="TCursor">The KeySet cursor type</typeparam>
    /// <param name="strategy">The KeySet pagination strategy to use for pagination.</param>
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
    public static async Task<PageResultAsync<T, TCursor>> ApplyPaginationAsync<T, TCursor>(
        IKeySetPaginationStrategy<T, TCursor> strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        TCursor? afterCursor,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never,
        CancellationToken cancellationToken = default)
        where TCursor : IKeySetCursor
    {
        return await InternalApplyPaginationAsync(
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
    /// Paginates and materializes the queryable using KeySet pagination.
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <typeparam name="TCursor">The KeySet cursor type</typeparam>
    /// <typeparam name="TStrategy">The KeySet pagination strategy type, which must enable serialization support by implementing the <see cref="IKeySetCursorSerializer{TCursor}"/> interface.</typeparam>
    /// <param name="strategy">The KeySet pagination strategy to use for pagination.</param>
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
    public static Task<PageResultAsync<T, TCursor>> ApplyPaginationAsync<T, TCursor, TStrategy>(
        TStrategy strategy,
        IQueryable<T> queryable,
        ToListAsync<T> asyncMaterializationFunc,
        CountAsync<T>? asyncCountFunc,
        string? afterCursorString,
        int pageSize,
        ComputeNextPage computeNextPage = ComputeNextPage.EveryPage,
        PaginationDirection paginationDirection = PaginationDirection.Forward,
        ComputeTotalCount computeTotalCount = ComputeTotalCount.Never,
        CancellationToken cancellationToken = default)
        where TCursor : class, IKeySetCursor
        where TStrategy : IKeySetPaginationStrategy<T, TCursor>, IKeySetCursorSerializer<TCursor>
    {
        ArgumentNullException.ThrowIfNull(strategy);
        var cursor = afterCursorString == null ? null : strategy.CursorFromString(afterCursorString);
        return InternalApplyPaginationAsync(
            strategy: strategy,
            queryable: queryable,
            asyncMaterializationFunc: asyncMaterializationFunc,
            asyncCountFunc: asyncCountFunc,
            afterCursor: cursor,
            pageSize: pageSize,
            computeNextPage: computeNextPage,
            totalCount: null,
            paginationDirection: paginationDirection,
            computeTotalCount: computeTotalCount,
            cancellationToken: cancellationToken);
    }
    #endregion
}
