namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class ErrorTests
{
    private const string ValidPaginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required bool BoolProp { get; set; }
    public required int IntProp { get; set; }
    public required string StringProp { get; set; }
}
""";

    private const string ValidPaginatedTypeWithObjectProperty = """
using System;

namespace TestNamespace;
public class TestInput
{
    public required object ObjectProperty { get; set; }
}
""";

    [Fact]
    public Task ErrorDiagnosticOnDuplicateProperty()
    {
        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([ValidPaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticOnDuplicateOrder()
    {
        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([ValidPaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticWhenKeySetAndOffsetPaginationAttributeOnSameType()
    {
        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[OffsetPaginationStrategy(typeof(TestInput))]
[PaginationProperty(0, nameof(TestInput.BoolProp), PaginationOrdering.Ascending)]
public partial class TestPaginationStrategy
{

}
""";

        return TestHelper.Verify([ValidPaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticWhenNoPropertiesDefined()
    {
        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([ValidPaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticOnInaccessibleProperty()
    {
        const string PaginatedTypeWithProtectedProperty = """
namespace TestNamespace;
public class TestInput
{
    public required bool ProtectedProperty { protected get; set; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.ProtectedProperty), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithProtectedProperty, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticOnInaccessibleField()
    {
        const string PaginatedTypeWithProtectedField = """
namespace TestNamespace;
public class TestInput
{
    protected bool ProtectedField;
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, "ProtectedField", PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithProtectedField, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticOnWriteOnlyProperty()
    {
        const string PaginatedTypeWithWriteOnlyProperty = """
namespace TestNamespace;
public class TestInput
{
    private string? _field;
    public string WriteOnlyProperty { set => _field = value; }
}
""";

        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.WriteOnlyProperty), PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([PaginatedTypeWithWriteOnlyProperty, StrategyDecl]);
    }

    [Fact]
    public Task ErrorDiagnosticOnUnknownPropertyName()
    {
        const string StrategyDecl = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, "PropertyThatDoesNotExist", PaginationOrdering.Ascending)]
public partial class TestKeysetPaginationStrategy
{

}
""";

        return TestHelper.Verify([ValidPaginatedType, StrategyDecl]);
    }

    [Fact]
    public Task StrategyNotPartial()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var nonPartialStrategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
public class InternalStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, nonPartialStrategy]);
    }

    [Fact]
    public Task StrategyDefinedAsNestedClass()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var nonPartialStrategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

public class WrappingClass
{
    [KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
    [PaginationProperty(0, nameof(TestInput.IntProp), PaginationOrdering.Ascending)]
    public partial class NestedStrategy
    {

    }
}
""";

        return TestHelper.Verify([paginatedType, nonPartialStrategy]);
    }

    [Fact]
    public Task PaginatedTypeDoesNotHaveComparisonOperatorsDefined_GeneratesCodeThatDoesNotCompile()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required object ObjectProp { get; set; }
}
""";

        var strategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.ObjectProp), PaginationOrdering.Ascending)]
public partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, strategy]);
    }

    [Fact]
    public Task ErrorDiagnosticOnUnboundGenericPaginatedType()
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

[KeySetPaginationStrategy(typeof(TestInput<>), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput<int>.Prop))]
internal partial class TestStrategy
{

}
""";

        return TestHelper.Verify([paginatedType, strategy]);
    }

    [Fact]
    public Task ErrorDiagnosticGenericStrategyType()
    {
        var paginatedType = """
namespace TestNamespace;
public class TestInput
{
    public required int IntProp { get; set; }
}
""";

        var strategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(TestInput), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, nameof(TestInput.IntProp))]
internal partial class TestStrategy<T>
{

}
""";

        return TestHelper.Verify([paginatedType, strategy]);
    }

    [Fact]
    public Task ErrorDiagnosticPaginatedTypeIsNonNamedType()
    {
        var strategy = $$"""
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(int[]), KeySetCursorSerializerGeneration.DoNotGenerate)]
[PaginationProperty(0, "does not matter")]
internal partial class TestStrategy
{

}
""";

        return TestHelper.Verify([strategy]);
    }
}
