namespace Jameak.CursorPagination.Abstractions.Exceptions;

/// <summary>
/// An exception that is thrown when the given Pagination arguments are invalid.
/// </summary>
public sealed class PaginationArgumentException : Exception
{
    /// <summary/>
    public PaginationArgumentException(string message) : base(message)
    {

    }
}
