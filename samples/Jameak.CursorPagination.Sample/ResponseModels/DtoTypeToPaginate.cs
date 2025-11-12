using System;

namespace Jameak.CursorPagination.Sample.ResponseModels;

public class DtoTypeToPaginate
{
    public required int Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Data { get; set; }
}
