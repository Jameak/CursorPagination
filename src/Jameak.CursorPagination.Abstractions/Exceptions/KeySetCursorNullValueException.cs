namespace Jameak.CursorPagination.Abstractions.Exceptions;

/// <summary>
/// An exception that is thrown when a value used to construct a KeySet cursor is null.
/// </summary>
public sealed class KeySetCursorNullValueException : Exception
{
    /// <summary/>
    public KeySetCursorNullValueException(string paramName)
        : base($"The KeySet cursor argument '{paramName}' is null. This is not supported, unless the KeySet has been generated with nullable coalescing enabled for this argument.")
    {
    }

    /// <summary/>
    public KeySetCursorNullValueException(string message, Exception inner) : base(message, inner)
    {
    }
}
