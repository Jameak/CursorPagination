using System.Diagnostics.CodeAnalysis;
using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Enums;

namespace Jameak.CursorPagination.Page;

/// <summary>
/// Contains the result of an asynchronous paginated query with associated metadata.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
/// <typeparam name="TCursor">The cursor type.</typeparam>
public sealed class PageResultAsync<T, TCursor> where TCursor : ICursor
{
    internal PageResultAsync(
        List<RowData<T, TCursor>> items,
        bool? hasNextPage,
        int? totalCount,
        NextPageAsync<T, TCursor> nextPageAsyncFunc,
        TCursor? nextCursor,
        Func<Task<bool>> hasPreviousPageAsyncFunc)
    {
        HasNextPage = hasNextPage;
        TotalCount = totalCount;
        Items = items;
        NextPageAsyncFunc = nextPageAsyncFunc;
        NextCursor = nextCursor;
        HasPreviousPageAsyncFunc = hasPreviousPageAsyncFunc;
    }

    /// <summary>
    /// The items in the page.
    /// </summary>
    public IReadOnlyList<RowData<T, TCursor>> Items { get; }

    /// <summary>
    /// <para>The total count of the dataset.</para>
    /// <para>The value is <see langword="null"/> if the <see cref="ComputeTotalCount.Never"/> option was specified.</para>
    /// </summary>
    public int? TotalCount { get; }

    /// <summary>
    /// <para>Whether a next page exists.</para>
    /// <para>The value is <see langword="null"/> if the <see cref="ComputeNextPage.Never"/> option was specified.</para>
    /// </summary>
    public bool? HasNextPage { get; }

    /// <summary>
    /// Whether the page is empty.
    /// </summary>
    [MemberNotNullWhen(false, nameof(NextCursor))]
    public bool IsEmpty => Items.Count == 0;

    /// <summary>
    /// <para>The cursor that can be used to retrieve the next page, taking the pagination direction into account.</para>
    /// <para>The value is <see langword="null"/> if the page is empty.</para>
    /// </summary>
    public TCursor? NextCursor { get; }

    private NextPageAsync<T, TCursor> NextPageAsyncFunc { get; }
    private Func<Task<bool>> HasPreviousPageAsyncFunc { get; }

    /// <summary>
    /// Asynchronously retrieves the next page, using the same pagination options as specified for this page.
    /// </summary>
    /// <returns>The next page of data.</returns>
    public async Task<PageResultAsync<T, TCursor>> NextPageAsync(CancellationToken cancellationToken = default) => await NextPageAsyncFunc(cancellationToken);

    /// <summary>
    /// Returns true when a previous page exists.
    /// </summary>
    /// <remarks>
    /// <para>Note that the definition of 'does a previous page exist' depends on the chosen pagination algorithm.</para>
    /// <para>For KeySet-pagination this returns <see langword="true"/> if data exists before the current page.</para>
    /// <para>For Offset-pagination this returns <see langword="true"/> if the current page is <b>not the first page</b>.</para>
    /// </remarks>
    public async Task<bool> HasPreviousPageAsync() => await HasPreviousPageAsyncFunc();
}
