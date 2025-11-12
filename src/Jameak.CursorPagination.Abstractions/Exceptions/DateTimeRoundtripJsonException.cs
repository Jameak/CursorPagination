namespace Jameak.CursorPagination.Abstractions.Exceptions;
/// <summary>
/// Represents errors that occur during DateTime JSON serialization/deserialization.
/// </summary>
public sealed class DateTimeRoundtripJsonException : Exception
{
    /// <summary/>
    public DateTimeRoundtripJsonException(string message) : base(message)
    {
    }

    /// <summary/>
    public DateTimeRoundtripJsonException(string message, Exception inner) : base(message, inner)
    {
    }
}
