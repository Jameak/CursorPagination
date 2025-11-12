using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Abstractions.KeySetPagination;
/// <summary>
/// Interface implemented by source generated KeySet pagination classes.
/// </summary>
public interface IKeySetPaginationStrategy<TUserType, TCursor> where TCursor : IKeySetCursor
{
    /// <summary>
    /// <para>Applies KeySet pagination to the given queryable with the given arguments.</para>
    /// 
    /// <para>Remember to call <see cref="PostProcessMaterializedResultInPlace"/> afterward with the same arguments as passed to this method.</para>
    /// </summary>
    /// <param name="queryable">The queryable to apply pagination to</param>
    /// <param name="pageSize">The size of the page</param>
    /// <param name="checkHasNextPage">Controls whether one extra element should be taken which allows post-processing logic to determine whether a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="afterCursor">The cursor to use as the starting point for the pagination. To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <returns>The input queryable with pagination applied</returns>
    /// <remarks>
    /// Calling this method will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.
    /// </remarks>
    public IQueryable<TUserType> ApplyPagination(
        IQueryable<TUserType> queryable,
        int pageSize,
        bool checkHasNextPage,
        PaginationDirection paginationDirection,
        TCursor? afterCursor);

    /// <summary>
    /// <para>Creates KeySet pagination Funcs for Where, OrderBy, and Take that match the given arguments.</para>
    /// 
    /// <para>For simple LINQ use-cases, use the <see cref="ApplyPagination"/> method instead as it
    /// applies the Funcs to your <see cref="IQueryable"/> in the correct order.</para>
    ///
    /// <para>After applying the pagination Funcs to your IQueryable, remember to call
    /// <see cref="PostProcessMaterializedResultInPlace"/> afterwards with the same arguments as passed to this method.</para>
    /// 
    /// <para><i>This method exists as an escape-hatch for advanced use-cases where the pagination Funcs
    /// have to be composed with other business-specific LINQ-methods in a very specific order
    /// for Entity Framework to be able to translate the LINQ method-pipeline to SQL.</i></para>
    /// </summary>
    /// <param name="pageSize">The size of the page</param>
    /// <param name="checkHasNextPage">Controls whether one extra element should be taken which
    /// allows post-processing logic to determine whether a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="afterCursor">The cursor to use as the starting point for the pagination.
    /// To retrieve the first page, pass in <see langword="null"/>.</param>
    /// <returns>A tuple of pagination Funcs that apply OrderBy, Skip, and Take</returns>
    /// <remarks>
    /// Applying the 'applyOrderExpr' func to an <see cref="IQueryable{T}"/> will override any OrderBy calls you have already applied to the <see cref="IQueryable{T}"/>.
    /// </remarks>
    public (Func<IQueryable<TUserType>, IQueryable<TUserType>> applyWhereExpr,
            Func<IQueryable<TUserType>, IOrderedQueryable<TUserType>> applyOrderExpr,
            Func<IQueryable<TUserType>, IQueryable<TUserType>> applyTake)
        BuildPaginationMethods(
        int pageSize,
        bool checkHasNextPage,
        PaginationDirection paginationDirection,
        TCursor? afterCursor);

    /// <summary>
    /// <para>
    /// Post-processes the given materialized result according to the given pagination arguments.
    /// These arguments must be the same as those passed to the <see cref="ApplyPagination"/>
    /// method for correct results.</para>
    ///
    /// This method...
    /// <list type="number">
    ///   <item>
    ///     orderes the results correctly according to the <see cref="PaginationDirection"/>,
    ///   </item>
    ///   <item>
    ///     determines if a next page exists
    ///   </item>
    ///   <item>
    ///     if a next page exists, the extra element retrieved from the data store is omitted from the output.
    ///   </item>
    /// </list>
    /// </summary>
    /// <param name="materializedResult">The result of materializing the paginated queryable. This list is mutated in-place.</param>
    /// <param name="pageSize">The size of the page</param>
    /// <param name="checkHasNextPage">Controls whether one extra element should be taken which
    /// allows post-processing logic to determine whether a next page exists.</param>
    /// <param name="paginationDirection">The pagination direction</param>
    /// <param name="hasNextPage">True if the <paramref name="checkHasNextPage"/> argument is true
    /// and the given <paramref name="materializedResult"/> data indicates a next page exists.</param>
    public void PostProcessMaterializedResultInPlace(
        List<TUserType> materializedResult,
        int pageSize,
        bool checkHasNextPage,
        PaginationDirection paginationDirection,
        out bool? hasNextPage);

    /// <summary>
    /// Creates a Cursor-instance from a given data-object.
    /// </summary>
    TCursor CreateCursor(TUserType data);
}
