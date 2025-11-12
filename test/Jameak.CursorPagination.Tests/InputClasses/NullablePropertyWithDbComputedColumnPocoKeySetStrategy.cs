using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(NullablePropertyWithDbComputedColumnPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(NullablePropertyWithDbComputedColumnPoco.ComputedIntProp), PaginationOrdering.Ascending)]
internal partial class NullablePropertyWithDbComputedColumnPocoKeySetStrategy
{
}
