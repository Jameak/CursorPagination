using System.Runtime.CompilerServices;
using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Enums;
using Jameak.CursorPagination.Page;

namespace Jameak.CursorPagination;
internal static class InternalPaginatorHelper
{
    internal sealed record EmptyNextPageState(
        int? TotalCount,
        Func<bool> HasPreviousPageFunc);

    internal sealed record EmptyNextPageStateAsync(
        int? TotalCount,
        Func<Task<bool>> HasPreviousPageAsyncFunc);

    internal static NextPage<TData, TCursor> DetermineNextPageFunc<TData, TCursor, TDataEntry>(
        Func<TCursor, NextPage<TData, TCursor>> nextPageFuncGenerator,
        Func<TDataEntry, TCursor> createCursor,
        TDataEntry? nextCursorElement,
        EmptyNextPageState emptyNextPageState,
        bool? hasNextPage,
        ComputeNextPage computeNextPage)
        where TCursor : ICursor
        where TDataEntry : class
    {
        if (nextCursorElement == null
            || CanSkipNextPageCheck(computeNextPage, hasNextPage))
        {
            return EmptyNextPage<TData, TCursor>(emptyNextPageState);
        }

        return nextPageFuncGenerator(createCursor(nextCursorElement));
    }

    internal static NextPageAsync<TData, TCursor> DetermineNextPageAsyncFunc<TData, TCursor, TDataEntry>(
        Func<TCursor, NextPageAsync<TData, TCursor>> nextPageAsyncFuncGenerator,
        Func<TDataEntry, TCursor> createCursor,
        TDataEntry? nextCursorElement,
        EmptyNextPageStateAsync emptyNextPageState,
        bool? hasNextPage,
        ComputeNextPage computeNextPage)
        where TCursor : ICursor
        where TDataEntry : class
    {
        if (nextCursorElement == null
            || CanSkipNextPageCheck(computeNextPage, hasNextPage))
        {
            return EmptyNextPageAsync<TData, TCursor>(emptyNextPageState);
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

    private static NextPage<T, TCursor> EmptyNextPage<T, TCursor>(
        EmptyNextPageState emptyNextPageState) where TCursor : ICursor
    {
        return () => new PageResult<T, TCursor>(
            [],
            hasNextPage: false,
            emptyNextPageState.TotalCount,
            EmptyNextPage<T, TCursor>(emptyNextPageState),
            nextCursor: default,
            emptyNextPageState.HasPreviousPageFunc);
    }

    private static NextPageAsync<T, TCursor> EmptyNextPageAsync<T, TCursor>(
        EmptyNextPageStateAsync emptyNextPageState) where TCursor : ICursor
    {
        return (cancellationToken) => Task.FromResult(
            new PageResultAsync<T, TCursor>(
                [],
                hasNextPage: false,
                emptyNextPageState.TotalCount,
                EmptyNextPageAsync<T, TCursor>(emptyNextPageState),
                nextCursor: default,
                emptyNextPageState.HasPreviousPageAsyncFunc));
    }

    internal static bool ShouldComputeTotalCount(bool hasAlreadyComputedCount, ComputeTotalCount totalEnum)
    {
        return (hasAlreadyComputedCount, totalEnum) switch
        {
            (_, ComputeTotalCount.Never) => false,
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

    internal static (T? previousCursorElement, T? nextCursorElement) GetCursorElements<T>(
        List<T> pageData,
        PaginationDirection paginationDirection)
    {
        return paginationDirection switch
        {
            PaginationDirection.Forward => (pageData.FirstOrDefault(), pageData.LastOrDefault()),
            PaginationDirection.Backward => (pageData.LastOrDefault(), pageData.FirstOrDefault()),
            _ => throw new ArgumentOutOfRangeException(nameof(paginationDirection)),
        };
    }

    internal static PaginationDirection InvertDirection(PaginationDirection paginationDirection)
    {
        return paginationDirection switch
        {
            PaginationDirection.Forward => PaginationDirection.Backward,
            PaginationDirection.Backward => PaginationDirection.Forward,
            _ => throw new ArgumentOutOfRangeException(nameof(paginationDirection)),
        };
    }
}
