using Jameak.CursorPagination.Sample.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Sample;

public static class DelegateMethods
{
    public static ToListAsync<DtoTypeToPaginate> ToListAsyncDelegate() => (queryable, cancellationToken) => queryable.ToListAsync(cancellationToken);
    public static CountAsync<DtoTypeToPaginate> CountAsyncDelegate() => (queryable, cancellationToken) => queryable.CountAsync(cancellationToken);
    public static AnyAsync<DtoTypeToPaginate> AnyAsyncDelegate() => (queryable, cancellationToken) => queryable.AnyAsync(cancellationToken);
}
