using System;
using System.Threading;
using System.Threading.Tasks;
using Jameak.CursorPagination.Page;
using Jameak.CursorPagination.Sample.DbModels;
using Jameak.CursorPagination.Sample.ResponseModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jameak.CursorPagination.Sample;

public class PaginatedBatchJob : BackgroundService
{
    private const int PageSize = 100;
    private readonly ILogger<PaginatedBatchJob> _logger;
    private readonly IServiceProvider _services;

    public PaginatedBatchJob(ILogger<PaginatedBatchJob> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background paginated batch job running.");

        await ProcessInBatches(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessInBatches(stoppingToken);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Background paginated batch job is stopping.");
        }
    }

    private async Task ProcessInBatches(CancellationToken stoppingToken)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
        var keySetPaginationStrategy = scope.ServiceProvider.GetRequiredService<KeySetPaginationStrategy>();
        // This sample job paginates through all the data in the db every loop.
        // Real use-cases would filter this to only the relevant data to be processed.
        var dtoQueryable = Mapper.ProjectToDto(dbContext.TypeToPaginate);

        var firstPage = await KeySetPaginator.ApplyPaginationAsync<DtoTypeToPaginate, KeySetPaginationStrategy.Cursor, KeySetPaginationStrategy>(
                keySetPaginationStrategy,
                dtoQueryable,
                DelegateMethods.ToListAsyncDelegate(),
                DelegateMethods.CountAsyncDelegate(),
                DelegateMethods.AnyAsyncDelegate(),
                null,
                PageSize,
                computeTotalCount: Enums.ComputeTotalCount.Once,
                computeNextPage: Enums.ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
                cancellationToken: stoppingToken);

        var totalPageCount = (int)Math.Ceiling(firstPage.TotalCount!.Value / (double)PageSize);

        var enumerator = new AsyncEnumerablePages<DtoTypeToPaginate, KeySetPaginationStrategy.Cursor>(firstPage);
        var currentPage = 0;
        // Note that the first loop processes the 'firstPage' value, as per the rules of the Enumerator interface.
        await foreach (var page in enumerator.WithCancellation(stoppingToken))
        {
            currentPage++;
            _logger.LogInformation("Background batch job is processing page {CurrentPage}/{TotalPageCount}", currentPage, totalPageCount);
            // Simulate work
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Background paginated batch job finished processing all pages");
    }
}
