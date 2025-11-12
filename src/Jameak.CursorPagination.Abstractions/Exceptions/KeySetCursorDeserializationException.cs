namespace Jameak.CursorPagination.Abstractions.Exceptions;
/// <summary>
/// Represents errors that occur during KeySet cursor deserialization.
/// </summary>
public sealed class KeySetCursorDeserializationException : Exception
{
    /// <summary/>
    public KeySetCursorDeserializationException(string message) : base(message)
    {
    }

    /// <summary/>
    public KeySetCursorDeserializationException(string message, Exception inner) : base(message, inner)
    {
    }
}
