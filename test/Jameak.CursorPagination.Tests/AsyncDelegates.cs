namespace Jameak.CursorPagination.Tests;
internal class AsyncDelegates
{
    public static Task<List<T>> SyncToList<T>(IQueryable<T> queryable)
    {
        return Task.FromResult(queryable.ToList());
    }

    public static Task<int> SyncCount<T>(IQueryable<T> queryable)
    {
        return Task.FromResult(queryable.Count());
    }

    public static Task<bool> SyncAny<T>(IQueryable<T> queryable)
    {
        return Task.FromResult(queryable.Any());
    }
}
