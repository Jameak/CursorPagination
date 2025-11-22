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

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static string UrlSafeBase64Encode(string toEncode)
    {
        // Based on https://github.com/dotnet/aspnetcore/blob/main/src/Shared/WebEncoders/WebEncoders.cs which is MIT licensed.
        // Reworked for .NET Standard 2.0
        var bytesToEncode = Encoding.UTF8.GetBytes(toEncode);
        var outputBuffer = new char[GetArraySizeRequiredToEncode(bytesToEncode.Length)];
        var numBase64Chars = Convert.ToBase64CharArray(bytesToEncode, 0, bytesToEncode.Length, outputBuffer, 0);

        for (var i = 0; i < numBase64Chars; i++)
        {
            var ch = outputBuffer[i];
            switch (ch)
            {
                case '+':
                    outputBuffer[i] = '-';
                    break;
                case '/':
                    outputBuffer[i] = '_';
                    break;
                case '=':
                    // We've reached a padding character; truncate the remainder.
                    return CreateString(outputBuffer, i);
            }
        }

        return CreateString(outputBuffer, numBase64Chars);

        static string CreateString(char[] outputBuffer, int length)
        {
            return new string(outputBuffer, startIndex: 0, length: length);
        }

        static int GetArraySizeRequiredToEncode(int count)
        {
            var numWholeOrPartialInputBlocks = checked(count + 2) / 3;
            return checked(numWholeOrPartialInputBlocks * 4);
        }
    }

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static string UrlSafeBase64Decode(string toDecode)
    {
        // Based on https://github.com/dotnet/aspnetcore/blob/main/src/Shared/WebEncoders/WebEncoders.cs which is MIT licensed.
        // Reworked for .NET Standard 2.0
        var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(toDecode.Length);
        var buffer = new char[checked(toDecode.Length + paddingCharsToAdd)];

        var i = 0;
        for (var j = 0; i < toDecode.Length; i++, j++)
        {
            var ch = toDecode[j];
            switch (ch)
            {
                case '-':
                    buffer[i] = '+';
                    break;
                case '_':
                    buffer[i] = '/';
                    break;
                default:
                    buffer[i] = ch;
                    break;
            }
        }

        for (; i < toDecode.Length + paddingCharsToAdd; i++)
        {
            buffer[i] = '=';
        }

        return Encoding.UTF8.GetString(Convert.FromBase64CharArray(buffer, 0, buffer.Length));

        static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
        {
            return (inputLength % 4) switch
            {
                2 => 2,
                3 => 1,
                _ => 0,
            };
        }
    }

    /// <summary>
    /// This is an internal API that supports the library infrastructure
    /// and not subject to the same compatibility standards as public APIs.
    /// It may be changed or removed without notice in any release.
    /// </summary>
    public static (
        Func<IQueryable<T>, IOrderedQueryable<T>> applyOrderExpr,
        Func<IQueryable<T>, IQueryable<T>> applySkip,
        Func<IQueryable<T>, IQueryable<T>> applyTake)
        OffsetBuildPaginationMethods<T>(
        int pageSize,
        bool checkHasNextPage,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderFunc,
        OffsetCursor? cursor)
    {
        ThrowIfPageSizeInvalid(pageSize, checkHasNextPage);

        Func<IQueryable<T>, IQueryable<T>> skipFunc;
        if (cursor != null)
        {
            var skipValue = cursor.Skip;
            skipFunc = queryable => queryable.Skip(skipValue);
        }
        else
        {
            skipFunc = queryable => queryable;
        }

        var toTake = pageSize;
        if (checkHasNextPage)
        {
            toTake = ComputeToTake(pageSize);
        }

        IQueryable<T> TakeFunc(IQueryable<T> queryable) => queryable.Take(toTake);

        return (orderFunc, skipFunc, TakeFunc);
    }
}
