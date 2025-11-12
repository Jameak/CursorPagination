using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.CursorPagination.Sample.DbModels;

public static class DbSeeder
{
    private const int NumElementsToSeed = 1_050;

    public static void InitializeAndSeed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

        dbContext.Database.EnsureCreated();
        var dataToSeed = InfiniteDataSource().Take(NumElementsToSeed);
        dbContext.TypeToPaginate.AddRange(dataToSeed);
        dbContext.SaveChanges();
    }

    private static IEnumerable<DbTypeToPaginate> InfiniteDataSource()
    {
        var id = 1;
        var createdAt = DateTime.UtcNow.AddDays(-1000);
        var random = new Random();
        while (true)
        {
            createdAt = createdAt.AddDays(1);
            yield return new DbTypeToPaginate()
            {
                Id = id++,
                CreatedAt = createdAt,
                Data = random.Next().ToString()
            };
        }
    }
}
