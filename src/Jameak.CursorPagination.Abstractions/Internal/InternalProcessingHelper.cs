using System.Text;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.Exceptions;
using Jameak.CursorPagination.Abstractions.OffsetPagination;

namespace Jameak.CursorPagination.Abstractions.Internal;

/// <summary>
/// This is an internal API that supports the library infrastructure
/// and not subject to the same compatibility standards as public APIs.
/// It may be changed or removed without notice in any release.
/// </summary>
[InternalUsageOnly]
public static class InternalProcessingHelper
{
    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static void ThrowIfPageSizeInvalid(int pageSize, bool checkHasNextPage)
    {
        if (pageSize < 0)
        {
            throw new PaginationArgumentException("Page size must be non-negative.");
        }

        if (pageSize == int.MaxValue && checkHasNextPage)
        {
            throw new PaginationArgumentException($"Page size cannot be set to 'int.MaxValue' when '{nameof(checkHasNextPage)}' is enabled as that would cause arithmetic overflow.");
        }
    }

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static IEnumerable<RowData<T, OffsetCursor>> OffsetPostProcessResult<T>(
        List<T> materializedResult,
        int pageSize,
        bool checkHasNextPage,
        PaginationDirection paginationDirection,
        OffsetCursor? cursor,
        out bool? hasNextPage)
    {
        if (materializedResult == null)
        {
            throw new ArgumentNullException(nameof(materializedResult));
        }

        if (!Enum.IsDefined(typeof(PaginationDirection), paginationDirection))
        {
            throw new ArgumentException($"Parameter '{nameof(paginationDirection)}' has invalid enum value '{paginationDirection}'.", nameof(paginationDirection));
        }

        ThrowIfPageSizeInvalid(pageSize, checkHasNextPage);
        IEnumerable<T> processedResult = materializedResult;
        hasNextPage = null;
        if (checkHasNextPage)
        {
            hasNextPage = materializedResult.Count >= ComputeToTake(pageSize);
            processedResult = hasNextPage.Value
                ? materializedResult.Take(pageSize)
                : materializedResult;
        }

        var pageStart = cursor?.Skip ?? 0;
        var resultWithCursors = processedResult.Select((row, index) =>
        {
            int skipValue;
            checked
            {
                skipValue = pageStart + index + 1;
            }

            return new RowData<T, OffsetCursor>(
                row,
                new OffsetCursor(skipValue));
        });

        if (paginationDirection == PaginationDirection.Backward)
        {
            return resultWithCursors.Reverse();
        }

        return resultWithCursors;
    }

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static void KeySetPostProcessResultInPlace<T>(
        List<T> materializedResult,
        int pageSize,
        bool checkHasNextPage,
        PaginationDirection paginationDirection,
        out bool? hasNextPage)
    {
        if (materializedResult == null)
        {
            throw new ArgumentNullException(nameof(materializedResult));
        }

        if (!Enum.IsDefined(typeof(PaginationDirection), paginationDirection))
        {
            throw new ArgumentException($"Parameter '{nameof(paginationDirection)}' has invalid enum value '{paginationDirection}'.", nameof(paginationDirection));
        }

        ThrowIfPageSizeInvalid(pageSize, checkHasNextPage);
        HandleNextPageCheckInPlace(materializedResult, pageSize, checkHasNextPage, out hasNextPage);

        if (paginationDirection == PaginationDirection.Backward)
        {
            materializedResult.Reverse();
        }
    }

    private static int ComputeToTake(int pageSize)
    {
        checked
        {
            return pageSize + 1;
        }
    }

    private static void HandleNextPageCheckInPlace<T>(
        List<T> materializedResult,
        int pageSize,
        bool checkHasNextPage,
        out bool? hasNextPage)
    {
        hasNextPage = null;

        if (checkHasNextPage)
        {
            hasNextPage = materializedResult.Count >= ComputeToTake(pageSize);
            if (hasNextPage.Value)
            {
                materializedResult.RemoveRange(pageSize, materializedResult.Count - pageSize);
            }
        }
    }

    private static readonly char[] s_base64Padding = ['='];

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static string UrlSafeBase64Encode(string toEncode)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode)).TrimEnd(s_base64Padding).Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static string UrlSafeBase64Decode(string toDecode)
    {
        // Copied from https://stackoverflow.com/a/26354677
        // License - CC BY-SA 3.0
        var incoming = toDecode.Replace('_', '/').Replace('-', '+');
        switch (toDecode.Length % 4)
        {
            case 2: incoming += "=="; break;
            case 3: incoming += "="; break;
        }
        var bytes = Convert.FromBase64String(incoming);
        return Encoding.ASCII.GetString(bytes);
    }
}
