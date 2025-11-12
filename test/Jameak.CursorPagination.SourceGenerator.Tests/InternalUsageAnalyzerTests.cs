namespace Jameak.CursorPagination.SourceGenerator.Tests;
public class InternalUsageAnalyzerTests
{
    [Fact]
    public Task UsingInternalOnlyClassReportsDiagnostic_MethodReference()
    {
        var userCode = """
using Jameak.CursorPagination.Abstractions.Internal;

public class UserClass
{
    public void Test()
    {
        InternalProcessingHelper.ThrowIfPageSizeInvalid(0, false);
    }
}
""";

        return TestHelper.Verify([userCode]);
    }

    [Fact]
    public Task UsingInternalOnlyClassReportsDiagnostic_TypeOf()
    {
        var userCode = """
using Jameak.CursorPagination.Abstractions.Internal;

public class UserClass
{
    public void Test()
    {
        var type = typeof(InternalProcessingHelper);
    }
}
""";

        return TestHelper.Verify([userCode]);
    }
}
