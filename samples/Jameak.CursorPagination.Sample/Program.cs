using System.Collections.Generic;
using System.Linq;
using Jameak.CursorPagination.Sample;
using Jameak.CursorPagination.Sample.DbModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddHostedService<PaginatedBatchJob>();
builder.Services.AddScoped<KeySetPaginationStrategy>();
builder.Services.AddScoped<OffsetPaginationStrategy>();
builder.Services.AddDbContext<SampleDbContext>(opt =>
{
    opt.UseSqlite("DataSource=file:memdb1?mode=memory&cache=shared");
    opt.EnableSensitiveDataLogging(true).EnableDetailedErrors(true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

DbSeeder.InitializeAndSeed(app.Services);

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    return string.Join("\n", endpointSources
        .SelectMany(source => source.Endpoints)
        .Select(endpoint => endpoint is RouteEndpoint route ? route.RoutePattern.RawText + " (" + route + ")" : endpoint.ToString()));
});

await app.RunAsync();
