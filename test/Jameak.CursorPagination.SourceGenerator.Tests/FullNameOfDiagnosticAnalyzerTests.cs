using Jameak.CursorPagination.SourceGenerator.Analyzers;

namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class FullNameOfDiagnosticAnalyzerTests
{
    [Fact]
    public async Task NameOfWithNestedMemberRefWithoutPrefixReportsDiagnostic()
    {
        var paginatedType = """
namespace TestNamespace;       

public class Level1<T>
{
    public required Level2 Nested { get; set; }
    public required T SomethingGeneric { get; set; }

public class Level2
{
    public required string Data1 { get; set; }
    public required string Data2 { get; set; }
    public required string Data3 { get; set; }
    public required string Data4 { get; set; }
    public required string Data5 { get; set; }
    public required string Data6 { get; set; }
    public required string Data7 { get; set; }
}
""";

        var strategyWithError = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(Level1<int>), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(Level1<int>.Nested.Data1), PaginationOrdering.Ascending)]
[PaginationProperty(1, Property: nameof(Level1<int>.Nested.Data2), PaginationOrdering.Ascending)]
[PaginationProperty(2, Property: true ? nameof(Level1<int>.Nested.Data3) : "test", PaginationOrdering.Ascending)]
[PaginationProperty(3, true ? nameof(Level1<int>.Nested.Data4) : "test", PaginationOrdering.Ascending)]
[PaginationProperty(4, nameof(global::TestNamespace.Level1<int>.Nested.Data5), PaginationOrdering.Ascending)]
[PaginationProperty(5, (nameof(Level1<int>.Nested.Data6)), PaginationOrdering.Ascending)]
[PaginationProperty(6, (string)nameof(Level1<int>.Nested.Data7), PaginationOrdering.Ascending)]
internal partial class TestStrategy
{
}
""";

        var fixedStrategy = """
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;

namespace TestNamespace;

[KeySetPaginationStrategy(typeof(Level1<int>), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@Level1<int>.Nested.Data1), PaginationOrdering.Ascending)]
[PaginationProperty(1, Property: nameof(@Level1<int>.Nested.Data2), PaginationOrdering.Ascending)]
[PaginationProperty(2, Property: true ? nameof(@Level1<int>.Nested.Data3) : "test", PaginationOrdering.Ascending)]
[PaginationProperty(3, true ? nameof(@Level1<int>.Nested.Data4) : "test", PaginationOrdering.Ascending)]
[PaginationProperty(4, nameof(global::@TestNamespace.Level1<int>.Nested.Data5), PaginationOrdering.Ascending)]
[PaginationProperty(5, (nameof(@Level1<int>.Nested.Data6)), PaginationOrdering.Ascending)]
[PaginationProperty(6, (string)nameof(@Level1<int>.Nested.Data7), PaginationOrdering.Ascending)]
internal partial class TestStrategy
{
}
""";

        await TestHelper.AssertDiagnosticsWithCodeFixer<FullNameOfDiagnosticCodeFixer>(strategyWithError, fixedStrategy, [paginatedType], [(DiagnosticHelper.s_suspiciousNameOfRule.Id, 7)]);

        // Fixed code should not report any diagnostics
        await TestHelper.AssertDiagnosticsWithCodeFixer<FullNameOfDiagnosticCodeFixer>(fixedStrategy, fixedStrategy, [paginatedType], []);
    }

    [Fact]
    public Task NameOfWithNestedMemberRefOutsidePaginationPropertyIsNotReported()
    {
        var types = """
namespace TestNamespace;

public class Level1
{
    public required Level2 Nested { get; set; }
}

public class Level2
{
    public required string Data { get; set; }
}

public class Program
{
    public string GetNameViaNameofWithoutPrefix()
    {
        // Should not report diagnostic.
        return nameof(Level1.Nested.Data);
    }
}

[KeySetPaginationStrategy(typeof(Level1), KeySetCursorSerializerGeneration.UseSystemTextJson)]
[PaginationProperty(0, nameof(@Level1.Nested.Data), PaginationOrdering.Ascending)]
internal partial class TestStrategy
{
    public void Test()
    {
        // Should not report diagnostic.
        return nameof(Level1.Nested.Data);
    }
}
""";

        return TestHelper.AssertDiagnosticsWithCodeFixer<FullNameOfDiagnosticCodeFixer>(types, types, [], []);
    }
}
