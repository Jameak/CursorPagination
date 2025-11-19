using Jameak.CursorPagination.Abstractions;

namespace Jameak.CursorPagination.Page;

/// <summary>
/// Exposes an enumerator that provides asynchronous iteration over the available pages.
/// </summary>
/// <remarks>
/// Facilitates iterating through the available pages via <i>await foreach</i>.
/// <code>
/// var pages = new AsyncEnumerablePages&lt;UserType, CursorType&gt;(firstPage);
/// await foreach (var page in pages)
/// {
///     // Use the page data
/// }
/// </code>
/// <para>The initial iteration of the enumerator produces the 'startPage' initial value, as per the rules of the Enumerator interface.</para>
/// </remarks>
/// <typeparam name="T">The type of the data.</typeparam>
/// <typeparam name="TCursor">The cursor type.</typeparam>
public sealed class AsyncEnumerablePages<T, TCursor> : IAsyncEnumerable<PageResultAsync<T, TCursor>> where TCursor : ICursor
{
    private readonly PageResultAsync<T, TCursor> _startPage;

    /// <summary/>
    public AsyncEnumerablePages(PageResultAsync<T, TCursor> startPage)
    {
        _startPage = startPage;
    }

    /// <summary>
    /// Returns an enumerator that iterates asynchronously through the pages.
    /// </summary>
    public IAsyncEnumerator<PageResultAsync<T, TCursor>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new KeySetPageAsyncEnumerator(_startPage, cancellationToken);

    private sealed class KeySetPageAsyncEnumerator : IAsyncEnumerator<PageResultAsync<T, TCursor>>
    {
        private readonly PageResultAsync<T, TCursor> _startPage;
        private readonly CancellationToken _cancellationToken;

        public KeySetPageAsyncEnumerator(PageResultAsync<T, TCursor> startPage, CancellationToken cancellationToken)
        {
            Current = default!;
            _startPage = startPage;
            _cancellationToken = cancellationToken;
        }

        public PageResultAsync<T, TCursor> Current { get; private set; }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Current == null)
            {
                Current = _startPage;
            }
            else
            {
                Current = await Current.NextPageAsync(_cancellationToken);
            }

            return !Current.IsEmpty;
        }
    }
}
