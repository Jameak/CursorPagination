using System.Collections;
using Jameak.CursorPagination.Abstractions;

namespace Jameak.CursorPagination.Page;

/// <summary>
/// Exposes an enumerator that provides synchronous iteration over the available pages.
/// </summary>
/// <remarks>
/// Facilitates iterating through the available pages via <i>foreach</i>.
/// <code>
/// var pages = new EnumerablePages&lt;UserType, CursorType&gt;(firstPage);
/// foreach (var page in pages)
/// {
///     // Use the page data
/// }
/// </code>
/// <para>The initial iteration of the enumerator produces the 'startPage' initial value, as per the rules of the Enumerator interface.</para>
/// </remarks>
/// <typeparam name="T">The type of the data.</typeparam>
/// <typeparam name="TCursor">The cursor type.</typeparam>
public sealed class EnumerablePages<T, TCursor> : IEnumerable<PageResult<T, TCursor>> where TCursor : ICursor
{
    private readonly PageResult<T, TCursor> _startPage;

    /// <summary/>
    public EnumerablePages(PageResult<T, TCursor> startPage)
    {
        _startPage = startPage;
    }

    /// <summary>
    /// Returns an enumerator that iterates synchronously through the pages.
    /// </summary>
    public IEnumerator<PageResult<T, TCursor>> GetEnumerator() => new KeySetPageEnumerator(_startPage);

    /// <summary>
    /// Returns an enumerator that iterates synchronously through the pages.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class KeySetPageEnumerator : IEnumerator<PageResult<T, TCursor>>
    {
        private readonly PageResult<T, TCursor> _startPage;
        private PageResult<T, TCursor>? _currentPage;

        public KeySetPageEnumerator(PageResult<T, TCursor> startPage)
        {
            _startPage = startPage;
            _currentPage = null;
        }

        public PageResult<T, TCursor> Current => _currentPage ?? throw new InvalidOperationException("Calling Current before having called MoveNext is not valid.");

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_currentPage == null)
            {
                _currentPage = _startPage;
            }
            else
            {
                _currentPage = _currentPage.NextPage();
            }

            return !_currentPage.IsEmpty;
        }
        public void Reset() => _currentPage = null;
    }
}
