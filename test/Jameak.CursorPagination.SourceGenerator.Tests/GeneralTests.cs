namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class GeneralTests
{
    [Fact]
    public Task EscapesPropertyWithReservedKeywordName()
    {
        var paginatedTypeWithReservedKeyword = """
namespace TestNamespace;
public class TestInput
{
    public required string @namespace { get; set; }
}
""";

        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.@namespace), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedTypeWithReservedKeyword, strategyDefinition]);
    }

    [Fact]
    public Task WorksWithFieldsInsteadOfProperties()
    {
        var paginatedTypeWithField = """
namespace TestNamespace;
public class TestInput
{
    public string PublicField;

    public TestInput()
    {
        PublicField = "test";
    }
}
""";

        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.PublicField), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedTypeWithField, strategyDefinition]);
    }

    [Fact]
    public Task WorksWithGetOnlyProperty()
    {
        var paginatedTypeWithGetOnlyProp = """
namespace TestNamespace;
public class TestInput
{
    public string GetOnlyProp { get; }

    public TestInput()
    {
        GetOnlyProp = "test";
    }
}
""";

        var strategyDefinition = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.GetOnlyProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedTypeWithGetOnlyProp, strategyDefinition]);
    }

    [Fact]
    public Task DuplicateStrategyClassNamesGetUniqueOutputNames()
    {
        var paginatedType = """
namespace TestNamespace.Input;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var strategyDefinitionOne = $$"""
using TestNamespace.Input;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace.Output.One;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        var strategyDefinitionTwo = $$"""
using TestNamespace.Input;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace.Output.Two;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, strategyDefinitionOne, strategyDefinitionTwo]);
    }

    [Fact]
    public Task InternalStrategy()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var internalStrategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
internal partial class InternalStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, internalStrategy]);
    }

    [Fact]
    public Task PaginationPropertyVariousOrdersAndDirections()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
    public required long LongProp { get; set; }
    public required string StringProp { get; set; }
}
""";

        var paginationStrategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(2, nameof(TestInput.IntProp), PaginationOrdering.Descending)]
[PaginationProperty(-5, nameof(TestInput.LongProp), PaginationOrdering.Ascending)]
[PaginationProperty(0, nameof(TestInput.StringProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, paginationStrategy]);
    }

    [Fact]
    public Task PartialStrategyTypeDeclaredInGlobalNamespace()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var paginationStrategy = $$"""
using TestNamespace;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, paginationStrategy]);
    }

    [Fact]
    public Task StrategyDeclaredInNestedNamespace()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var nestedNsStrategy = $$"""
using TestNamespace;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TopLevel
{
    namespace Nested
    {
        [KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
        [PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
        public partial class NestedNsStrategy
        {

        }
    }
}
""";

        return TestHelper.Verify([paginatedType, nestedNsStrategy]);
    }

    [Fact]
    public Task GenericPaginatedType()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput<T>
{
    public T Prop { get; set; } = default!;
}
""";

        var strategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput<int>), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(TestInput<int>.Prop))]
internal partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, strategy]);
    }
}
