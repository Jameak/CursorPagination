using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Sample.DbModels;

public class SampleDbContext : DbContext
{
    public DbSet<DbTypeToPaginate> TypeToPaginate { get; set; }

    public SampleDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbTypeToPaginate>()
            .HasIndex(e => new { e.CreatedAt, e.Id })
            .IsDescending(true, false);
    }
}
