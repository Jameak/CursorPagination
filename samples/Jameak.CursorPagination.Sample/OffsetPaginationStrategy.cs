using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Sample.ResponseModels;

namespace Jameak.CursorPagination.Sample;

// Configure Offset source generation
[OffsetPaginationStrategy(typeof(DtoTypeToPaginate))]
// Define the columns and their sort order. Supports composite ordering and mixing ascending/descending
[PaginationProperty(Order: 0, nameof(DtoTypeToPaginate.CreatedAt), PaginationOrdering.Descending)]
[PaginationProperty(Order: 1, nameof(DtoTypeToPaginate.Id), PaginationOrdering.Ascending)]
public partial class OffsetPaginationStrategy
{
}
