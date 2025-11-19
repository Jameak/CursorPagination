using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Page;

namespace Jameak.CursorPagination;
#pragma warning disable MA0048 // File name must match type name

/// <summary>
/// Method used to asynchronously materialize the paginated <see cref="IQueryable{T}"/> to a list.
/// </summary>
/// <returns>The materialized list.</returns>
/// <remarks>
/// If using EFCore for async operations you can create this async materialization method like so:
/// <code>
/// (queryable, cancellationToken) => queryable.ToListAsync(cancellationToken)
/// </code>
/// </remarks>
public delegate Task<List<T>> ToListAsync<T>(IQueryable<T> paginatedQueryable, CancellationToken cancellationToken);

/// <summary>
/// Method used to asynchronously count the number of elements in the <see cref="IQueryable{T}"/>.
/// </summary>
/// <returns>The number of elements.</returns>
/// <remarks>
/// If using EFCore for async operations you can create this async count method like so:
/// <code>
/// (queryable, cancellationToken) => queryable.CountAsync(cancellationToken)
/// </code>
/// </remarks>
public delegate Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken);

/// <summary>
/// Method used to asynchronously determine whether a <see cref="IQueryable{T}"/> contains any elements.
/// </summary>
/// <returns>Returns true if any elements exist.</returns>
/// <remarks>
/// If using EFCore for async operations you can create this async any method like so:
/// <code>
/// (queryable, cancellationToken) => queryable.AnyAsync(cancellationToken)
/// </code>
/// </remarks>
public delegate Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken);

internal delegate PageResult<T, TCursor> NextPage<T, TCursor>() where TCursor : ICursor;

internal delegate Task<PageResultAsync<T, TCursor>> NextPageAsync<T, TCursor>(CancellationToken cancellationToken) where TCursor : ICursor;
