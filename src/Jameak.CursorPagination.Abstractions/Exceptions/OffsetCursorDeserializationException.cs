namespace Jameak.CursorPagination.Abstractions.Exceptions;
/// <summary>
/// Represents errors that occur during Offset cursor deserialization.
/// </summary>
public sealed class OffsetCursorDeserializationException : Exception
{
    /// <summary/>
    public OffsetCursorDeserializationException(string message) : base(message)
    {
    }

    /// <summary/>
    public OffsetCursorDeserializationException(string message, Exception inner) : base(message, inner)
    {
    }
}
