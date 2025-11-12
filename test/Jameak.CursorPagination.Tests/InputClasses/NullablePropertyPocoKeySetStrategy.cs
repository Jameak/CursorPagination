using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Tests.InputClasses;

[KeySetPaginationStrategy(typeof(NullablePropertyPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(NullablePropertyPoco.NullableIntProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(NullablePropertyPoco.NullableGuidProp), PaginationOrdering.Descending)]
[PaginationProperty(2, nameof(NullablePropertyPoco.NullableBoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(3, nameof(NullablePropertyPoco.NullableStringProp), PaginationOrdering.Ascending)]
internal partial class NullablePropertyPocoKeySetStrategy
{
}
