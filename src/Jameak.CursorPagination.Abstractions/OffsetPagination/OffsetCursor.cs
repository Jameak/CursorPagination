using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Jameak.CursorPagination.Abstractions.Exceptions;
using Jameak.CursorPagination.Abstractions.Internal;

namespace Jameak.CursorPagination.Abstractions.OffsetPagination;
/// <summary>
/// A cursor used for Offset pagination
/// </summary>
public sealed record OffsetCursor : ICursor
{
    /// <summary>
    /// The Skip-value represented by this cursor
    /// </summary>
    public int Skip { get; }

    /// <summary>
    /// Constructs a new Offset cursor with the given Skip-value.
    /// </summary>
    /// <param name="skip">The number of elements to skip</param>
    /// <exception cref="ArgumentException">Throw if the given skip-argument is negative.</exception>
    public OffsetCursor(int skip)
    {
        if (skip < 0)
        {
            throw new ArgumentException("Skip value must be non-negative.", nameof(skip));
        }

        Skip = skip;
    }

    /// <summary>
    /// Serializes the cursor to an opaque cursor string.
    /// </summary>
    public string CursorToString()
    {
        return InternalProcessingHelper.UrlSafeBase64Encode(Skip.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Constructs a cursor from an opaque cursor string
    /// </summary>
    /// <param name="cursorString">An opaque string representing an Offset cursor</param>
    /// <exception cref="OffsetCursorDeserializationException">Thrown if the given cursor cannot be created from the given string</exception>
    public static OffsetCursor CursorFromString(string cursorString)
    {
        try
        {
            return new OffsetCursor(int.Parse(InternalProcessingHelper.UrlSafeBase64Decode(cursorString), NumberStyles.None, CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            throw new OffsetCursorDeserializationException($"Failed to create an instance of {nameof(OffsetCursor)} from the given '{nameof(cursorString)}' argument value", ex);
        }
    }

    /// <summary>
    /// Constructs a cursor from an opaque cursor string. A return value indicates whether the construction succeeded.
    /// </summary>
    /// <param name="cursorString">An opaque cursor string representing an Offset cursor</param>
    /// <param name="cursor">The constructed cursor if construction succeeded</param>
    /// <returns><code>True</code> if the construction succeeded</returns>
    public static bool TryCursorFromString(string cursorString, [NotNullWhen(true)] out OffsetCursor? cursor)
    {
        cursor = null;
        try
        {
            cursor = CursorFromString(cursorString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
