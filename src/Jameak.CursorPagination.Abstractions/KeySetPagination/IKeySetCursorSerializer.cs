namespace Jameak.CursorPagination.Abstractions.KeySetPagination;
/// <summary>
/// Interface implemented by source generated KeySet pagination classes when source-generation of cursor serialization-support has been enabled.
/// </summary>
public interface IKeySetCursorSerializer<TCursor> where TCursor : IKeySetCursor
{
    /// <summary>
    /// Creates a <typeparamref name="TCursor"/> instance from an opaque cursor string.
    /// </summary>
    TCursor CursorFromString(string cursorString);

    /// <summary>
    /// Serializes a <typeparamref name="TCursor"/> instance to an opaque cursor string.
    /// </summary>
    string CursorToString(TCursor cursor);
}
