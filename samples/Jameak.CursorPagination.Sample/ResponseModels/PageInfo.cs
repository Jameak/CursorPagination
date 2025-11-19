namespace Jameak.CursorPagination.Sample.ResponseModels;

public class PageInfo
{
    public required string? NextPageCursor { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
    public required int TotalCount { get; init; }
}
