namespace Jameak.CursorPagination.Abstractions.Exceptions;
/// <summary>
/// Represents errors that occur during KeySet cursor serialization.
/// </summary>
public sealed class KeySetCursorSerializationException : Exception
{
    /// <summary/>
    public KeySetCursorSerializationException(string message) : base(message)
    {
    }

    /// <summary/>
    public KeySetCursorSerializationException(string message, Exception inner) : base(message, inner)
    {
    }
}
