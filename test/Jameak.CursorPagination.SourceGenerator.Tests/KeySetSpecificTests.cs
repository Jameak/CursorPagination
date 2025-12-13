namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class KeySetSpecificTests
{
    private static readonly string s_paginatedType = """
using System;

namespace TestNamespace;
public class TestInput
{
    public required bool BoolProp { get; set; }
    public required byte ByteProp { get; set; }
    public required DateTime DateTimeProp { get; set; }
    public required DateTimeOffset DateTimeOffsetProp { get; set; }
    public required decimal DecimalProp { get; set; }
    public required double DoubleProp { get; set; }
    public required float FloatProp { get; set; }
    public required Guid GuidProp { get; set; }
    public required int IntProp { get; set; }
    public required long LongProp { get; set; }
    public required sbyte SbyteProp { get; set; }
    public required short ShortProp { get; set; }
    public required string StringProp { get; set; }
    public required ushort UshortProp { get; set; }
    public required uint UintProp { get; set; }
    public required ulong UlongProp { get; set; }
}
""";

    [Fact]
    public static Task VerifyTokenSerializationEnabled()
    {
        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(1, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(2, nameof(TestInput.ByteProp), PaginationOrdering.Ascending)]
[PaginationProperty(3, nameof(TestInput.DateTimeProp), PaginationOrdering.Ascending)]
[PaginationProperty(4, nameof(TestInput.DateTimeOffsetProp), PaginationOrdering.Ascending)]
[PaginationProperty(5, nameof(TestInput.DecimalProp), PaginationOrdering.Ascending)]
[PaginationProperty(6, nameof(TestInput.DoubleProp), PaginationOrdering.Ascending)]
[PaginationProperty(7, nameof(TestInput.FloatProp), PaginationOrdering.Ascending)]
[PaginationProperty(8, nameof(TestInput.GuidProp), PaginationOrdering.Ascending)]
[PaginationProperty(9, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
[PaginationProperty(10, nameof(TestInput.LongProp), PaginationOrdering.Ascending)]
[PaginationProperty(11, nameof(TestInput.SbyteProp), PaginationOrdering.Ascending)]
[PaginationProperty(12, nameof(TestInput.ShortProp), PaginationOrdering.Ascending)]
[PaginationProperty(13, nameof(TestInput.StringProp), PaginationOrdering.Ascending)]
[PaginationProperty(14, nameof(TestInput.UshortProp), PaginationOrdering.Ascending)]
[PaginationProperty(15, nameof(TestInput.UintProp), PaginationOrdering.Ascending)]
[PaginationProperty(16, nameof(TestInput.UlongProp), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.VerifySourceGeneration([s_paginatedType, strategyDefinition]);
    }
}
