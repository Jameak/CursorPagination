namespace Jameak.CursorPagination.Enums;
/// <summary>
/// Possible ways to configure the 'compute the total count' logic.
/// </summary>
public enum ComputeTotalCount
{
    /// <summary>
    /// The total count will never be computed.
    /// </summary>
    Never = 1,
    /// <summary>
    /// The total count will be computed once on the first page, and the value copied to every subsequent page.
    /// </summary>
    Once = 2,
    /// <summary>
    /// The total count will be re-computed on every page.
    /// </summary>
    EveryPage = 3,
}
