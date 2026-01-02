namespace Jameak.CursorPagination.Enums;
/// <summary>
/// Possible ways to configure the 'compute whether a next page exists' logic.
/// </summary>
public enum ComputeNextPage
{
    /// <summary>
    /// Whether a next page exists will never be pre-computed.
    /// </summary>
    Never = 1,
    /// <summary>
    /// Whether a next page exists will be pre-computed on every page.
    /// </summary>
    EveryPage = 2,
    /// <summary>
    /// <para>Whether a next page exists will be pre-computed on every page.</para>
    /// <para>Additionally, with this option set, when the 'next page' computation determines that no next page exists,
    /// attempting to retrieve the next page will skip the database round-trip and directly return an empty page.</para>
    /// </summary>
    EveryPageAndPreventNextPageQueryOnLastPage = 3,
}
