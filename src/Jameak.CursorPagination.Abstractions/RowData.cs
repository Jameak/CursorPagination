namespace Jameak.CursorPagination.Abstractions;

/// <summary>
/// Container type for a page item with an associated cursor value.
/// </summary>
public sealed record RowData<T, TCursor> where TCursor : ICursor
{
    /// <summary>
    /// The row data.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// The cursor associated with the data.
    /// </summary>
    public TCursor Cursor { get; }

    /// <summary>
    /// Constructs a row with data and its associated cursor value.
    /// </summary>
    public RowData(T data, TCursor cursor)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
    }
}
