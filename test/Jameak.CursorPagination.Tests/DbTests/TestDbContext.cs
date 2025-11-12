using Jameak.CursorPagination.Tests.InputClasses;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests.DbTests;
public class TestDbContext : DbContext
{
    private readonly List<string> _logMessages = [];

    public DbSet<SimplePropertyPoco> SimplePropertyTestTable { get; set; }
    public DbSet<NullablePropertyWithDbComputedColumnPoco> ComputedNullableTestTable { get; set; }
    public DbSet<NullablePropertyPoco> NullableTestTable { get; set; }
    public DbSet<SimpleFieldPoco> SimpleFieldTestTable { get; set; }

    public IEnumerable<string> LogMessages => _logMessages;

    public TestDbContext(DbContextOptions options) : base(options)
    {

    }

    public void ClearLogMessages() => _logMessages.Clear();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NullablePropertyWithDbComputedColumnPoco>()
            .Property(e => e.ComputedIntProp)
            .HasComputedColumnSql($"COALESCE({nameof(NullablePropertyWithDbComputedColumnPoco.NullableIntProp)}, {int.MinValue})");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(message =>
        {
            lock (_logMessages)
            {
                _logMessages.Add(message);
            }
        },
        Microsoft.Extensions.Logging.LogLevel.Information,
        Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.Level
        | Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.Category
        | Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.Id);
    }
}
