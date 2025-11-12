using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Sample.ResponseModels;

namespace Jameak.CursorPagination.Sample;

// Configure KeySet source generation
[KeySetPaginationStrategy(typeof(DtoTypeToPaginate), CursorSerialization: KeySetCursorSerializerGeneration.UseSystemTextJson)]
// Define the columns and their sort order. Supports composite keysets and mixing ascending/descending
[PaginationProperty(Order: 0, nameof(DtoTypeToPaginate.CreatedAt), PaginationOrdering.Descending)]
[PaginationProperty(Order: 1, nameof(DtoTypeToPaginate.Id), PaginationOrdering.Ascending)]
public partial class KeySetPaginationStrategy
{
}
