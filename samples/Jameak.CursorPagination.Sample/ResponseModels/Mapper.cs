using System.Linq;
using Jameak.CursorPagination.Sample.DbModels;

namespace Jameak.CursorPagination.Sample.ResponseModels;

public static class Mapper
{
    public static IQueryable<DtoTypeToPaginate> ProjectToDto(IQueryable<DbTypeToPaginate> queryable)
    {
        return queryable.Select(e => new DtoTypeToPaginate
        {
            Id = e.Id,
            CreatedAt = e.CreatedAt,
            Data = e.Data,
        });
    }
}
