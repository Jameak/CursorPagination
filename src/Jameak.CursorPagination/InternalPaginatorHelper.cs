using System.Runtime.CompilerServices;
using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;

namespace Jameak.CursorPagination;
internal static class InternalPaginatorHelper
{
    internal static NextPage<TData, TCursor> DetermineNextPageFunc<TData, TCursor, TDataEntry>(
        Func<TCursor, NextPage<TData, TCursor>> nextPageFuncGenerator,
        Func<TDataEntry, TCursor> createCursor,
        TDataEntry? nextCursorElement,
        int? totalCount,
        bool? hasNextPage,
        ComputeNextPage computeNextPage) where TCursor : ICursor
    {
        if (nextCursorElement == null
            || CanSkipNextPageCheck(computeNextPage, hasNextPage))
        {
            return EmptyNextPage<TData, TCursor>(totalCount);
        }

        return nextPageFuncGenerator(createCursor(nextCursorElement));
    }

    internal static NextPageAsync<TData, TCursor> DetermineNextPageAsyncFunc<TData, TCursor, TDataEntry>(
        Func<TCursor, NextPageAsync<TData, TCursor>> nextPageAsyncFuncGenerator,
        Func<TDataEntry, TCursor> createCursor,
        TDataEntry? nextCursorElement,
        int? totalCount,
        bool? hasNextPage,
        ComputeNextPage computeNextPage) where TCursor : ICursor
    {
        if (nextCursorElement == null
            || CanSkipNextPageCheck(computeNextPage, hasNextPage))
        {
            return EmptyNextPageAsync<TData, TCursor>(totalCount);
        }

        return nextPageAsyncFuncGenerator(createCursor(nextCursorElement));
    }

    private static bool CanSkipNextPageCheck(
        ComputeNextPage computeNextPage,
        bool? hasNextPage)
    {
        return computeNextPage == ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage
              && hasNextPage.HasValue
              && !hasNextPage.Value;
    }

    internal static NextPage<T, TCursor> EmptyNextPage<T, TCursor>(int? totalCount) where TCursor : ICursor
    {
        return () => new PageResult<T, TCursor>([], false, totalCount, EmptyNextPage<T, TCursor>(totalCount), default);
    }

    internal static NextPageAsync<T, TCursor> EmptyNextPageAsync<T, TCursor>(int? totalCount) where TCursor : ICursor
    {
        return (cancellationToken) => Task.FromResult(new PageResultAsync<T, TCursor>([], false, totalCount, EmptyNextPageAsync<T, TCursor>(totalCount), default));
    }

    internal static bool ShouldComputeTotalCount(bool hasAlreadyComputedCount, ComputeTotalCount totalEnum)
    {
        return (hasAlreadyComputedCount, totalEnum) switch
        {
            (false, ComputeTotalCount.Never) => false,
            (false, ComputeTotalCount.Once) => true,
            (true, ComputeTotalCount.Once) => false,
            (_, ComputeTotalCount.EveryPage) => true,
            _ => throw new NotSupportedException($"Unhandled switch case: {hasAlreadyComputedCount}. Should never happen, {totalEnum}"),
        };
    }

    internal static void ThrowIfEnumNotDefined<T>(T argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) where T : struct, Enum
    {
        if (!Enum.IsDefined(argument))
        {
            throw new ArgumentException($"Parameter '{paramName}' has invalid enum value '{argument}'.", paramName);
        }
    }
}
