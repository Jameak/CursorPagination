namespace Jameak.CursorPagination.Sample.ResponseModels;

public class DataWithCursor
{
    public required string Cursor { get; init; }
    public required DtoTypeToPaginate Data { get; init; }
}
