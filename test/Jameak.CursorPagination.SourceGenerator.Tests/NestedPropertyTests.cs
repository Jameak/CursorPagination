namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class NestedPropertyTests
{
    [Fact]
    public Task NullabilityVariety_KeySetAndOffset()
    {
        var paginatedType = """
namespace TestNamespace;

public record Level1
{
    public required Level2 Nested { get; set; }
}

public record Level2
{
    public required Level3 NullableNested { get; set; }
}

public record Level3
{
    public required Level4 ValueTypeNested { get; set; }
}

public struct Level4
{
    public required Level5? NullableValueTypeNested { get; set; }
}

public struct Level5
{
    public required string StringProp { get; set; }
    public required int IntProp { get; set; }
}
""";

        var keysetStrategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(Level1), KeySetCursorSerializerGeneration.UseSystemTextJson)]
// Specifying using full nameof
[PaginationProperty(0, nameof(@Level1.Nested.NullableNested.ValueTypeNested.NullableValueTypeNested.Value.StringProp), PaginationOrdering.Ascending)]
// Specifying via string
[PaginationProperty(1, "Nested.NullableNested.ValueTypeNested.NullableValueTypeNested.Value.IntProp", PaginationOrdering.Ascending)]
internal partial class TestStrategyKeySet
{
}
""";
        var offsetStrategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[OffsetPaginationStrategy(typeof(Level1))]
// Specifying using full nameof
[PaginationProperty(0, nameof(@Level1.Nested.NullableNested.ValueTypeNested.NullableValueTypeNested.Value.StringProp), PaginationOrdering.Ascending)]
// Specifying via string
[PaginationProperty(1, "Nested.NullableNested.ValueTypeNested.NullableValueTypeNested.Value.IntProp", PaginationOrdering.Ascending)]
internal partial class TestStrategyOffset
{
}
""";

        return TestHelper.VerifySourceGeneration([paginatedType, keysetStrategy, offsetStrategy]);
    }

    [Fact]
    public Task NestedWithGenericAndNamespaces()
    {
        var paginatedType = """
namespace Test.Some.Deep.Namespace;

public interface INestedViaGeneric<E>
{
    E Prop { get; set; }
}

public record Level1<T, E> where T : INestedViaGeneric<E>
{
    public required T Nested { get; set; }
}

public record Level2<E> : INestedViaGeneric<E>
{
    public required E Prop { get; set; }
}
""";

        var strategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace OtherNamespace;

[KeySetPaginationStrategy(typeof(Test.Some.Deep.Namespace.Level1<Test.Some.Deep.Namespace.Level2<int>, int>), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@Test.Some.Deep.Namespace.Level1<Test.Some.Deep.Namespace.Level2<int>, int>.Nested.Prop), PaginationOrdering.Ascending)]
internal partial class TestStrategy
{
}
""";

        return TestHelper.VerifySourceGeneration([paginatedType, strategy]);
    }

    [Fact]
    public Task NestedClasses()
    {
        var paginatedType = """
namespace TestNamespace;       

public class Level1
{
    public class Level2
    {
        public required Level3 Nested { get; set; }
    }

    public class Level3
    {
        public required string Data1 { get; set; }
        public required string Data2 { get; set; }
    }
}

""";

        var strategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(Level1.Level2), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@Level1.Level2.Nested.Data1), PaginationOrdering.Ascending)]
[PaginationProperty(1, nameof(global::@TestNamespace.Level1.Level2.Nested.Data2), PaginationOrdering.Ascending)]
internal partial class TestStrategy
{
}
""";

        return TestHelper.VerifySourceGeneration([paginatedType, strategy]);
    }

    [Fact]
    public Task ReservedKeywords()
    {
        var paginatedType = """
namespace TestNamespace;

public record @event
{
    public required @public @namespace { get; set; }
}

public record @public
{
    public required string @namespace { get; set; }
    public required string @event { get; set; }
}
""";

        var keysetStrategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(@event), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@event.@namespace.@namespace), PaginationOrdering.Ascending)]
[PaginationProperty(1, "@namespace.@event", PaginationOrdering.Ascending)]
internal partial class @private
{
}
""";
        var offsetStrategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(@event), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@event.@namespace.@namespace), PaginationOrdering.Ascending)]
[PaginationProperty(1, "@namespace.@event", PaginationOrdering.Ascending)]
internal partial class @protected
{
}
""";

        return TestHelper.VerifySourceGeneration([paginatedType, keysetStrategy, offsetStrategy]);
    }
}
