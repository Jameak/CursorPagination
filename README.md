# <img src="images/icon.png" alt="Icon" width="25" height="25"> Jameak.CursorPagination
[![CI](https://github.com/Jameak/CursorPagination/actions/workflows/ci.yml/badge.svg)](https://github.com/Jameak/CursorPagination/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/dt/Jameak.CursorPagination?label=NuGet)](https://www.nuget.org/packages/Jameak.CursorPagination/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

An easy-to-use efficient KeySet- and Offset-pagination C# implementation for `IQueryable` with included opaque Cursor support, all enabled by a compile-time Source Generator.

The library is tested using EFCore, but may work with other ORMs too as the library has no EFCore dependencies.

## Terms
Different frameworks and articles use different names for the same pagination concepts, so to avoid confusion here is how this library uses these terms:

Pagination approaches:
* __Offset__ pagination (aka. SQL-queries with `OFFSET`, aka. `Skip/Take` LINQ-methods): Paginating through the dataset by skipping a given number of rows.
* __KeySet__ pagination (aka. seek-based pagination, aka. cursor-pagination): Paginating through the dataset by retrieving data _after the last item from the previous page_.

A __Cursor__ (aka. pagination token, aka. continuation token) is used to indicate the current position in the dataset. A cursor is usually an opaque value that lets you fetch the next or previous page without knowing the underlying data structure or pagination solution.

## Offset vs KeySet pagination
This library supports both Offset and KeySet pagination.

If you're not familiar with the differences, see [this article](https://learn.microsoft.com/en-us/ef/core/querying/pagination) for an overview, and [this deep dive](https://use-the-index-luke.com/no-offset) for why Offset pagination can become inefficient as your dataset grows.

In summary, you should always use KeySet pagination unless your use-case requires random page access.

## Usage

> [!TIP]
> Every API shown below also has an async equivalent.
>
> See the [sample application](samples/Jameak.CursorPagination.Sample) for additional examples.

### Configuring pagination options
This library uses a source-generator to create optimized pagination functions and strongly-typed cursor-classes at compile-time. To get started, define a partial class and add the attributes that describe how your data should be paginated:
```csharp
class TypeToPaginate
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Data { get; set; }
}

// Configure KeySet source generation
[KeySetPaginationStrategy(typeof(TypeToPaginate), CursorSerialization: KeySetCursorSerializerGeneration.UseSystemTextJson)]
// Define the columns and their sort order. Supports composite keysets and mixing ascending/descending
[PaginationProperty(Order: 0, nameof(TypeToPaginate.CreatedAt), PaginationOrdering.Descending)]
[PaginationProperty(Order: 1, nameof(TypeToPaginate.Id), PaginationOrdering.Ascending)]
partial class PaginationStrategy;
```

For Offset pagination, use `[OffsetPaginationStrategy(...)]` instead.

### Retrieving the first page
Once the pagination strategy is generated, you can use it with the built-in end-to-end pagination API:
```csharp
var paginationStrategy = new PaginationStrategy();
IQueryable<TypeToPaginate> queryable = /* your data source*/;

// Retrieve the first page:
var firstPage = KeySetPaginator.ApplyPagination(
    paginationStrategy,
    queryable,
    afterCursor: null, // null means "retrieve the first page"
    pageSize: 10,
    paginationDirection: PaginationDirection.Forward, // Use 'Backward' to paginate backwards.
    computeTotalCount: ComputeTotalCount.Never); // Use 'Once' to include the total count.
```

For Offset pagination, use `OffsetPaginator` instead.

### Retrieving the cursor pointing to the next page as an opaque string
Each returned page-object includes a helper that exposes the cursor used to retrieve the next page.

#### KeySet pagination
```csharp
var nextPageCursor = firstPage.NextCursor;
if (nextPageCursor == null) {
    return /* No more pages */
} else {
    return paginationStrategy.CursorToString(nextPageCursor);
}
```

#### Offset pagination
```csharp
var nextPageCursor = firstPage.NextCursor;
if (nextPageCursor == null) {
    return /* No more pages */
} else {
    return nextPageCursor.CursorToString();
}
```

### Retrieving the next page
To retrieve the next page using a cursor string, call the same API again:
```csharp
// Second page, using the above cursor string:
var nextPage = KeySetPaginator.ApplyPagination<TypeToPaginate, PaginationStrategy.Cursor, PaginationStrategy>(
    paginationStrategy,
    queryable,
    afterCursorString: nextPageCursorString,
    pageSize: 10,
    computeNextPage: ComputeNextPage.EveryPage,
    paginationDirection: PaginationDirection.Forward,
    computeTotalCount: ComputeTotalCount.Once);
```

### Using pagination in batch jobs
For background tasks or batch jobs that need to process large datasets in chunks, you can automatically iterate through pages without managing the cursors manually:
```csharp
var firstPage = /* Retrieve first page as shown above */
var pageEnumerator = new EnumerablePages<TypeToPaginate, PaginationStrategy.Cursor>(firstPage);
foreach (var page in pageEnumerator)
{
    // Process each page
}
```

## Opaque cursor strings
Both Offset and KeySet pagination rely on cursors to represent the current position in the dataset. Cursor strings are designed to be opaque since callers dont need to know what they contain, but simply that the cursor can be used to retrieve the next page.

For Offset-pagination, the cursor represents the numeric position of the given row and is represented by the `OffsetCursor` class.

For KeySet-pagination, the library generates a strongly typed cursor class at compile time based on your pagination configuration.
For the source-generated `PaginationStrategy` type configuration shown [above](#Usage), the following cursor class is generated which contains only the fields in the `[PaginationProperty]` attributes of the strategy:
```csharp
public sealed record Cursor
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
}
```
These generated cursor classes can be easily serialized and deserialized via the built-in System.Text.Json generation support. The source generator also generates a custom NamingPolicy so property names are hidden within the encoded cursor-string.

To make cursors opaque the library base64url-encodes them when converting them to strings. If your cursor use-case necessitates a different cursor encoding, it is simple to avoid using this built-in functionality while still taking advantage of the central pagination logic of the library.

## Deterministic ordering
To ensure correct pagination your data must be ordered deterministically, and your ordering must __never__ produce ties that the database could resolve differently between queries.

If your ordering isn't deterministic, then ...
* Offset pagination may skip or repeat rows.
* KeySet pagination may skip rows.

To ensure a deterministic ordering you __must__ include enough columns in your pagination ordering to uniquely identify each row. This is typically done by adding a known-to-be-unique key as the final tie-breaker. For example:
```csharp
[PaginationProperty(Order: 0, nameof(TypeToPaginate.CreatedAt), PaginationOrdering.Descending)]
[PaginationProperty(Order: 1, nameof(TypeToPaginate.Id), PaginationOrdering.Ascending)]
```

Here, the `CreatedAt` property defines the main sort order of interest, and the `Id` guarantees uniqueness.


## KeySet pagination with Nullable columns
Null is a special value in databases and does not behave consistently across database vendors.

This library aims to support nullable columns, provided that 'null'-values do not actually exist in your dataset.

This library _works_ with 'null'-values in the dataset, as long as your null-columns are not value types, but it is not generally supported. Making this work also requires that your ORM of choice (EFCore or others) generates SQL that correctly takes column nullability into account in a way that matches your chosen database because this library does nothing special to handle it.

If your use-case requires pagination over a nullable column, it is recommended that you avoid any potential null-issues by using one of these approaches:
* Add a computed column to your database table and use this column in your KeySet. Computed columns are typically defined directly in your SQL DDL or via your ORM. [See here for EF Core documentation on how to do this.](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=data-annotations#computed-columns)
* Or by coalescing nullable columns in the pagination configuration as shown below.

### Coalescing nullable columns
Coalescing nullable columns in the pagination configuration can be done by specifying a fallback value in the PaginationProperty attribute:
```csharp
[PaginationProperty(0, nameof(TypeToPaginate.NullableInt), PaginationOrdering.Ascending, "int.MaxValue")]
```
The string you provide is inserted on the right-hand side of a null-coalescing `??` operator in the generated query.

This string must:
* Be valid C# code
* Work as the right-hand side of a `??` expression.
* Be valid inside an `Expression` and translatable by your ORM.
* Use fully-qualified type-names where necessary

If any of these conditions are not met, the source generator may produce code that does not compile.

> [!IMPORTANT]
> For best performance, make sure your database has matching indexes for any coalesced expressions you use in your pagination configuration.

## Understanding Backward pagination
_Backward_ pagination is paginating a specific ordered dataset from the last-page to the first-page _with each page having its elements ordered in the forward direction_.

Given the ordered dataset `[A, B, C, D, E, F, G]` and a pagesize of 3, _forward_ pagination would produce the pages
* Page 1: `[A, B, C]`
* Page 2: `[D, E, F]`
* Page 3: `[G]`

Given the same ordered dataset and the same pagesize, _backward_ pagination would produce the pages
* Page "1": `[E, F, G]`
* Page "2": `[B, C, D]`
* Page "3": `[A]`

Notice that backward pagination walks through the dataset in reverse, but each individual page still shows its items in the forward order.

If you need the data in reverse order, use forward pagination with the opposite sort-order pagination configuration instead.

> [!IMPORTANT]
> Backward pagination requires an index that matches the opposite sort order of the forward pagination query.

### Non-materializing pagination
For use-cases that requires the pagination to __not__ materialize the dataset so that the IQueryable can be further combined, such as when paginating nested collections, it is possible to apply pagination to the `IQueryable` using the generated pagination class:
```
IQueryable<TypeToPaginate> paginatedQueryable = paginationStrategy.ApplyPagination(
    queryable,
    pageSize: 100,
    checkHasNextPage: true,
    paginationDirection: PaginationDirection.Forward,
    afterCursor: null);
```

Once the query has been materialized, you must remember to call the appropriate post-processing method:
* KeySet pagination: Call `PostProcessMaterializedResultInPlace`.
* Offset pagination: Call `PostProcessMaterializedResult`.

Pass the same arguments you used in the `ApplyPagination` call.

If even more granular control over how the pagination `Expressions` are applied to your `IQueryable` is needed, you can use the `BuildPaginationMethods` method on the generated pagination class to directly obtain `Func`s that apply LINQ `where`, `order`, `skip`, or `take` expressions when invoked.

## Minimum requirements
The source generator in this library uses Microsoft.CodeAnalysis.CSharp version 4.11.0, which imposes a requirement of .NET SDK version 8.0.4xx or newer on consumers, as well as Visual Studio 2022 version 17.11 or newer.
