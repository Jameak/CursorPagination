using System.Collections.Generic;

namespace Jameak.CursorPagination.Sample.ResponseModels;

public class PaginatedResponse
{
    public required PageInfo PageInfo { get; init; }
    public required List<DataWithCursor> Data { get; init; }
}
