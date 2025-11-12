namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class OffsetSpecificTests
{
    private static readonly string s_paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required bool BoolProp { get; set; }
    public required int IntProp { get; set; }
    public required long LongProp { get; set; }
}
""";

    [Fact]
    public Task VerifyOffsetGeneration_AllAscending()
    {
        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[OffsetPaginationStrategy(typeof(TestInput))]
[PaginationProperty(0, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public partial class TestOffsetPaginationStrategy
{

}
""";

        return TestHelper.Verify([s_paginatedType, strategyDefinition]);
    }

    [Fact]
    public Task VerifyOffsetGeneration_MixedOrdering()
    {
        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[OffsetPaginationStrategy(typeof(TestInput))]
[PaginationProperty(0, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(TestInput.IntProp), PaginationOrdering.Descending)]
public partial class TestOffsetPaginationStrategy
{

}
""";

        return TestHelper.Verify([s_paginatedType, strategyDefinition]);
    }

    [Fact]
    public Task VerifyOffsetGeneration_MixedOrderingAndFlippedOrderInteger()
    {
        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[OffsetPaginationStrategy(typeof(TestInput))]
[PaginationProperty(1, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Descending)]
public partial class TestOffsetPaginationStrategy
{

}
""";

        return TestHelper.Verify([s_paginatedType, strategyDefinition]);
    }
}
