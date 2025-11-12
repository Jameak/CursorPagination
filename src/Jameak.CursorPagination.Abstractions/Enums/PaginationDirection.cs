namespace Jameak.CursorPagination.Abstractions.Enums;
/// <summary>
/// The possible pagination directions
/// </summary>
public enum PaginationDirection
{
    /// <summary>
    /// Indicates Forward pagination.
    /// </summary>
    /// <remarks>
    /// Given the ordered dataset <code>[A, B, C, D, E, F, G]</code> and a pagesize of 3, forward pagination would produce the pages
    /// <code>
    /// Page 1: [A, B, C]
    /// Page 2: [D, E, F]
    /// Page 3: [G]
    /// </code>
    /// </remarks>
    Forward = 1,
    /// <summary>
    /// Indicates Backward pagination.
    /// </summary>
    /// <remarks>
    /// Given the ordered dataset <code>[A, B, C, D, E, F, G]</code> and a pagesize of 3, backward pagination would produce the pages
    /// <code>
    /// Page 1: [E, F, G]
    /// Page 2: [B, C, D]
    /// Page 3: [A]
    /// </code>
    /// <para>Notice that this produces the dataset paginated backwards, but with each page representing the data in the non-reverse order.</para>
    /// <para>If you need the data in reverse order, instead perform a Forward pagination on the opposite sort-order instead (ascending vs. descending)</para>
    /// </remarks>
    Backward = 2
}
