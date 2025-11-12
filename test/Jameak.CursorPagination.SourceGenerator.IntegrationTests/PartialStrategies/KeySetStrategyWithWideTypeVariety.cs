using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests.PartialStrategies;

[KeySetPaginationStrategy(typeof(WideTypeVarietyPoco), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(WideTypeVarietyPoco.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(WideTypeVarietyPoco.ByteProp), PaginationOrdering.Ascending)]
[PaginationProperty(2, nameof(WideTypeVarietyPoco.DateTimeProp), PaginationOrdering.Ascending)]
[PaginationProperty(3, nameof(WideTypeVarietyPoco.DateTimeOffsetProp), PaginationOrdering.Ascending)]
[PaginationProperty(4, nameof(WideTypeVarietyPoco.DecimalProp), PaginationOrdering.Ascending)]
[PaginationProperty(5, nameof(WideTypeVarietyPoco.DoubleProp), PaginationOrdering.Ascending)]
[PaginationProperty(6, nameof(WideTypeVarietyPoco.FloatProp), PaginationOrdering.Ascending)]
[PaginationProperty(7, nameof(WideTypeVarietyPoco.GuidProp), PaginationOrdering.Ascending)]
[PaginationProperty(8, nameof(WideTypeVarietyPoco.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(9, nameof(WideTypeVarietyPoco.LongProp), PaginationOrdering.Ascending)]
[PaginationProperty(10, nameof(WideTypeVarietyPoco.SbyteProp), PaginationOrdering.Ascending)]
[PaginationProperty(11, nameof(WideTypeVarietyPoco.ShortProp), PaginationOrdering.Ascending)]
[PaginationProperty(12, nameof(WideTypeVarietyPoco.StringProp), PaginationOrdering.Ascending)]
[PaginationProperty(13, nameof(WideTypeVarietyPoco.UshortProp), PaginationOrdering.Ascending)]
[PaginationProperty(14, nameof(WideTypeVarietyPoco.UintProp), PaginationOrdering.Ascending)]
[PaginationProperty(15, nameof(WideTypeVarietyPoco.UlongProp), PaginationOrdering.Ascending)]
[PaginationProperty(16, nameof(WideTypeVarietyPoco.NullableDateTimeProp), PaginationOrdering.Ascending, "System.DateTime.MinValue")]
internal partial class KeySetStrategyWithWideTypeVariety
{
}
