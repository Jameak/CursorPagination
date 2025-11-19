using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.DbClasses;
public sealed class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TestDbContext> _contextOptions;

    public required string DbCreateScript { get; init; }

    public DatabaseFixture()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        // Create the schema and seed some data
        using var context = new TestDbContext(_contextOptions);
        DbCreateScript = context.Database.GenerateCreateScript();
        context.Database.EnsureCreated();

        context.AddRange(TestHelper.CreateSimplePropertyPocoData());
        context.AddRange(TestHelper.CreateSimpleFieldPocoData());
        context.SaveChanges();
    }

    public TestDbContext CreateDbContext() => new(_contextOptions);
    public void Dispose() => _connection.Dispose();
}
