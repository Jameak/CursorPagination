using System.Runtime.CompilerServices;

namespace Jameak.CursorPagination.SourceGenerator.Tests;
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initializer()
    {
        Verifier.UseSourceFileRelativeDirectory("__snapshots__");
        VerifySourceGenerators.Initialize();
    }
}
