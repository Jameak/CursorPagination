namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class NullableTests
{
    [Fact]
    public Task PaginatedTypeWithNullables()
    {
        var paginatedTypeWithNullables = """
namespace TestNamespace;
public class TestInput
{
    public required string? NullableStringProp { get; set; }
}
""";

        var paginationStrategy = $$"""
using TestNamespace;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.NullableStringProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedTypeWithNullables, paginationStrategy]);
    }

    [Fact]
    public Task NullableProperty_NoNullCoalesce()
    {
        // Nullable class type (string) and nullable value type properties
        const string PaginatedTypeWithNullableProperty = """
using System;

namespace TestNamespace;
public class TestInput
{
    public required int? NullableIntProp { get; set; }
    public required Guid? NullableGuidProp { get; set; }
    public required bool? NullableBoolProp { get; set; }
    public required string? NullableStringProp { get; set; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput.NullableIntProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(TestInput.NullableGuidProp), PaginationOrdering.Ascending)]
[PaginationProperty(2, nameof(TestInput.NullableBoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(3, nameof(TestInput.NullableStringProp), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithNullableProperty, StrategyDecl]);
    }

    [Fact]
    public Task NullableProperty_WithNullCoalesce()
    {
        // Nullable class type (string) and nullable value type properties
        const string PaginatedTypeWithNullableProperty = """
using System;

namespace TestNamespace;
public class TestInput
{
    public required int? NullableIntProp { get; set; }
    public required Guid? NullableGuidProp { get; set; }
    public required bool? NullableBoolProp { get; set; }
    public required string? NullableStringProp { get; set; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput.NullableIntProp), PaginationOrdering.Ascending, "int.MinValue")]
[PaginationProperty(1, nameof(TestInput.NullableGuidProp), PaginationOrdering.Ascending, "System.Guid.Parse(\"abcdabcd-abcd-abcd-abcd-abcdabcdabcd\")")]
[PaginationProperty(2, nameof(TestInput.NullableBoolProp), PaginationOrdering.Ascending, "false")]
[PaginationProperty(3, nameof(TestInput.NullableStringProp), PaginationOrdering.Ascending, "string.Empty")]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithNullableProperty, StrategyDecl]);
    }

    [Fact]
    public Task NullableFields()
    {
        const string PaginatedTypeWithNullableProperty = """
using System;

namespace TestNamespace;
public class TestInput
{
    public int? NullableIntProp;
    public Guid? NullableGuidProp;
    public bool? NullableBoolProp;
    public string? NullableStringProp;
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput.NullableIntProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(TestInput.NullableGuidProp), PaginationOrdering.Ascending)]
[PaginationProperty(2, nameof(TestInput.NullableBoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(3, nameof(TestInput.NullableStringProp), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithNullableProperty, StrategyDecl]);
    }

    [Fact]
    public Task NonNullablePropertyWithNullCoalesceHasWarningDiagnostic()
    {
        const string PaginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required string StringProp { get; set; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput.StringProp), PaginationOrdering.Ascending, "string.Empty")]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task NonNullablePropertyWithNullCoalesceAndNullObliviousEmitsNoWarning()
    {
        const string PaginatedType = """
namespace TestNamespace;
public class TestInput
{
#nullable disable
    public required string StringProp { get; set; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput.StringProp), PaginationOrdering.Ascending, "string.Empty")]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedType, StrategyDecl]);
    }
}
