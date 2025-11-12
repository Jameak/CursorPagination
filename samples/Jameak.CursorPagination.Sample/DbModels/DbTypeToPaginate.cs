using System;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Sample.DbModels;

[PrimaryKey(nameof(Id))]
public class DbTypeToPaginate
{
    public required int Id { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Data { get; set; }
}
