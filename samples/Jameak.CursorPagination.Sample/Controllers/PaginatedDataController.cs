using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jameak.CursorPagination.Sample.DbModels;
using Jameak.CursorPagination.Sample.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Jameak.CursorPagination.Sample.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class PaginatedDataController : ControllerBase
{
    private const int PageSize = 100;

    private readonly SampleDbContext _dbContext;
    private readonly KeySetPaginationStrategy _keySetPaginationStrategy;
    private readonly OffsetPaginationStrategy _offsetPaginationStrategy;

    public PaginatedDataController(
        SampleDbContext dbContext,
        KeySetPaginationStrategy keySetPaginationStrategy,
        OffsetPaginationStrategy offsetPaginationStrategy)
    {
        _dbContext = dbContext;
        _keySetPaginationStrategy = keySetPaginationStrategy;
        _offsetPaginationStrategy = offsetPaginationStrategy;
    }

    private IQueryable<DtoTypeToPaginate> GetDtoQueryable() => Mapper.ProjectToDto(_dbContext.TypeToPaginate);

    /// <summary>
    /// Paginates through the available pages via a Cursor, using KeySet pagination.
    /// </summary>
    [HttpGet]
    public async Task<PaginatedResponse> PaginateByKeySetCursor(string? afterCursor, CancellationToken token)
    {
        var page = await KeySetPaginator.ApplyPaginationAsync<DtoTypeToPaginate, KeySetPaginationStrategy.Cursor, KeySetPaginationStrategy>(
            _keySetPaginationStrategy,
            GetDtoQueryable(),
            DelegateMethods.ToListAsyncDelegate(),
            DelegateMethods.CountAsyncDelegate(),
            afterCursor,
            PageSize,
            computeTotalCount: Enums.ComputeTotalCount.Once,
            computeNextPage: Enums.ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            cancellationToken: token);

        return new PaginatedResponse
        {
            PageInfo = new PageInfo
            {
                // Entirely up to your use-case how much of this page metadata makes sense to compute and fill out.
                TotalCount = page.TotalCount!.Value,
                HasNextPage = page.HasNextPage!.Value,
                NextPageCursor = page.NextCursor == null ? null : _keySetPaginationStrategy.CursorToString(page.NextCursor)
            },
            Data = page.Items.Select(item => new DataWithCursor
            {
                Cursor = _keySetPaginationStrategy.CursorToString(item.Cursor),
                Data = item.Data
            }).ToList()
        };
    }

    /// <summary>
    /// Paginates through the available pages via a Cursor, using Offset pagination.
    /// </summary>
    [HttpGet]
    public async Task<PaginatedResponse> PaginateByOffsetCursor(string? afterCursor, CancellationToken token)
    {
        var page = await OffsetPaginator.ApplyPaginationAsync(
            _offsetPaginationStrategy,
            GetDtoQueryable(),
            DelegateMethods.ToListAsyncDelegate(),
            DelegateMethods.CountAsyncDelegate(),
            afterCursor,
            PageSize,
            computeTotalCount: Enums.ComputeTotalCount.Once,
            computeNextPage: Enums.ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            cancellationToken: token);

        return new PaginatedResponse
        {
            PageInfo = new PageInfo
            {
                TotalCount = page.TotalCount!.Value,
                HasNextPage = page.HasNextPage!.Value,
                NextPageCursor = page.NextCursor?.CursorToString()
            },
            Data = page.Items.Select(item => new DataWithCursor
            {
                Cursor = item.Cursor.CursorToString(),
                Data = item.Data
            }).ToList()
        };
    }

    /// <summary>
    /// Paginates through the available pages via a page number. Only possible when using Offset pagination.
    /// </summary>
    [HttpGet]
    public async Task<PaginatedResponse> PaginateByPageNumber(int? pageNumber, CancellationToken token)
    {
        var page = await OffsetPaginator.ApplyPaginationAsync(
            _offsetPaginationStrategy,
            GetDtoQueryable(),
            DelegateMethods.ToListAsyncDelegate(),
            DelegateMethods.CountAsyncDelegate(),
            pageNumber ?? 1,
            PageSize,
            computeTotalCount: Enums.ComputeTotalCount.Once,
            computeNextPage: Enums.ComputeNextPage.EveryPageAndPreventNextPageQueryOnLastPage,
            cancellationToken: token);

        return new PaginatedResponse
        {
            PageInfo = new PageInfo
            {
                TotalCount = page.TotalCount!.Value,
                HasNextPage = page.HasNextPage!.Value,
                NextPageCursor = page.NextCursor?.CursorToString()
            },
            Data = page.Items.Select(item => new DataWithCursor
            {
                Cursor = item.Cursor.CursorToString(),
                Data = item.Data
            }).ToList()
        };
    }
}
