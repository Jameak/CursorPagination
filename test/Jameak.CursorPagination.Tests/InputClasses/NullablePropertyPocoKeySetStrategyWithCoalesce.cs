using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(NullablePropertyPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(NullablePropertyPoco.NullableIntProp), PaginationOrdering.Ascending, "int.MinValue")]
[PaginationProperty(1, nameof(NullablePropertyPoco.NullableGuidProp), PaginationOrdering.Descending, "System.Guid.Parse(\"abcdabcd-abcd-abcd-abcd-abcdabcdabcd\")")]
[PaginationProperty(2, nameof(NullablePropertyPoco.NullableBoolProp), PaginationOrdering.Ascending, "false")]
[PaginationProperty(3, nameof(NullablePropertyPoco.NullableStringProp), PaginationOrdering.Ascending, "string.Empty")]
internal partial class NullablePropertyPocoKeySetStrategyWithCoalesce
{
}
