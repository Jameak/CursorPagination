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
        TCursor? nextCursor)
    {
        HasNextPage = hasNextPage;
        TotalCount = totalCount;
        Items = items;
        NextPageAsyncFunc = nextPageAsyncFunc;
        NextCursor = nextCursor;
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

    /// <summary>
    /// Asynchronously retrieves the next page, using the same pagination options as specified for this page.
    /// </summary>
    /// <returns>The next page of data.</returns>
    public Task<PageResultAsync<T, TCursor>> NextPageAsync(CancellationToken cancellationToken = default) => NextPageAsyncFunc(cancellationToken);
}
