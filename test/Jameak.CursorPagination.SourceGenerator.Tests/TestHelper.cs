using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Basic.Reference.Assemblies;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jameak.CursorPagination.SourceGenerator.Tests;

public static class TestHelper
{
    private static readonly ImmutableArray<DiagnosticAnalyzer> s_analyzers = [new InternalUsageDiagnosticAnalyzer()];

    private static readonly HashSet<string> s_allTrackingNames = typeof(TrackingNames)
        .GetFields()
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        .Select(x => (string)x.GetRawConstantValue()!)
        .Where(x => !string.IsNullOrEmpty(x))
        .Select(e => e!)
        .ToHashSet();

    public static Task Verify([StringSyntax("csharp")] string sourceCode, [CallerFilePath] string callerFilePath = "")
    {
        return Verify([sourceCode], callerFilePath);
    }

    public static async Task Verify(IEnumerable<string> sourceText, [CallerFilePath] string callerFilePath = "")
    {
        var settings = new VerifySettings();
        settings.UseDirectory(Path.Combine(Path.GetDirectoryName(callerFilePath)!, "__snapshots__"));

        IEnumerable<PortableExecutableReference> references = [
            .. Net80.References.All,
            // Jameak.CursorPagination.Abstractions
            MetadataReference.CreateFromFile(typeof(IKeySetCursor).Assembly.Location)
            ];

        // Compilation errors are localized, so to ensure snapshot reproducibility we force a consistent culture.
        using var cultureScope = new ChangeCultureScope("en-US");

        var compileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithSpecificDiagnosticOptions(s_analyzers.SelectMany(e => e.SupportedDiagnostics).Select(diag => new KeyValuePair<string, ReportDiagnostic>(diag.Id, GetReportDiagnostic(diag))));
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: sourceText.Select(sourceCode => CSharpSyntaxTree.ParseText(sourceCode)),
            references: references,
            compileOptions);
        var generator = new PaginationGenerator();
        var generatorDriverOptions = new GeneratorDriverOptions(disabledOutputs: IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], driverOptions: generatorDriverOptions);
        var updatedDriverWithResults = driver.RunGenerators(compilation);

        AssertGeneratorOutputIsCacheable(compilation, updatedDriverWithResults);

        var analyzerCompilation = compilation.WithAnalyzers(s_analyzers);
        var analyzerDiags = await analyzerCompilation.GetAnalyzerDiagnosticsAsync(CancellationToken.None);

        // Generator reported error diagnostics
        if (updatedDriverWithResults.GetRunResult().Results
            .SelectMany(e => e.Diagnostics.Where(e => e.DefaultSeverity == DiagnosticSeverity.Error))
            .Any())
        {
            await Verifier.Verify(updatedDriverWithResults, settings)
                .AppendAnalyzerDiagsIfAny(analyzerDiags);
            return;
        }

        // No error diagnostics reported.
        // Compile the generated code to check that the emitted code is actually valid
        var updatedCompilation = compilation.AddSyntaxTrees(
            updatedDriverWithResults.GetRunResult().Results
            .SelectMany(r => r.GeneratedSources)
            .Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText, CSharpParseOptions.Default, gs.HintName)));

        using var dll = new MemoryStream();
        var emitted = updatedCompilation.Emit(dll);

        await Verifier.Verify(updatedDriverWithResults, settings)
            .AppendValue("generated-code-can-compile", emitted.Success)
            .AppendValue("generated-code-compilation-diagnostics", emitted.Diagnostics)
            .AppendAnalyzerDiagsIfAny(analyzerDiags);
    }

    private static SettingsTask AppendAnalyzerDiagsIfAny(this SettingsTask settingsTask, ImmutableArray<Diagnostic> analyzerDiags)
    {
        if (analyzerDiags.Length > 0)
        {
            settingsTask.AppendValue("analyzer-diagnostics", analyzerDiags);
        }

        return settingsTask;
    }

    private static ReportDiagnostic GetReportDiagnostic(DiagnosticDescriptor descriptor)
    {
        return descriptor.DefaultSeverity switch
        {
            DiagnosticSeverity.Hidden => ReportDiagnostic.Hidden,
            DiagnosticSeverity.Info => ReportDiagnostic.Info,
            DiagnosticSeverity.Warning => ReportDiagnostic.Warn,
            DiagnosticSeverity.Error => ReportDiagnostic.Error,
            _ => throw new NotImplementedException($"Unhandled severity: {descriptor.DefaultSeverity}")
        };
    }

    // Heavily based on https://andrewlock.net/creating-a-source-generator-part-10-testing-your-incremental-generator-pipeline-outputs-are-cacheable/
    private static void AssertGeneratorOutputIsCacheable(
        CSharpCompilation originalCompilation,
        GeneratorDriver originalDriver)
    {
        var originalRunResults = originalDriver.GetRunResult();
        var cloneCompilation = originalCompilation.Clone();
        var newRunResults = originalDriver.RunGenerators(cloneCompilation).GetRunResult();

        Assert.True(newRunResults
            .Results
            .SelectMany(e => e
                .TrackedOutputSteps
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Outputs))
            .All(e => e.Reason == IncrementalStepRunReason.Cached));

        var firstRunTrackedSteps = GetTrackedSteps(originalRunResults);
        var secondRunTrackedSteps = GetTrackedSteps(newRunResults);

        Assert.Equal(firstRunTrackedSteps.Count, secondRunTrackedSteps.Count);
        Assert.True(firstRunTrackedSteps.Keys.ToHashSet().SetEquals(secondRunTrackedSteps.Keys.ToHashSet()));

        foreach (var (trackingName, firstRunSteps) in firstRunTrackedSteps)
        {
            var secondRunSteps = secondRunTrackedSteps[trackingName];
            AssertRunStepsAreEqual(firstRunSteps, secondRunSteps);
        }

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(GeneratorDriverRunResult runResult)
        {
            return runResult
                .Results
                .SelectMany(e => e
                    .TrackedSteps
                    .Where(step => s_allTrackingNames.Contains(step.Key)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        static void AssertRunStepsAreEqual(
            ImmutableArray<IncrementalGeneratorRunStep> firstRunSteps,
            ImmutableArray<IncrementalGeneratorRunStep> secondRunSteps)
        {
            Assert.Equal(firstRunSteps.Length, secondRunSteps.Length);

            for (var i = 0; i < firstRunSteps.Length; i++)
            {
                var firstRunStep = firstRunSteps[i];
                var secondRunStep = secondRunSteps[i];

                var firstRunOutputs = firstRunStep.Outputs.Select(e => e.Value);
                var secondRunOutputs = secondRunStep.Outputs.Select(e => e.Value);

                Assert.Equal(firstRunOutputs, secondRunOutputs);
                Assert.True(secondRunStep.Outputs.All(e => e.Reason == IncrementalStepRunReason.Cached || e.Reason == IncrementalStepRunReason.Unchanged));

                foreach (var output in firstRunStep.Outputs)
                {
                    AssertObjectGraphDoesNotContainSymbolsOrCompilationData(output.Value);
                }
            }
        }

        static void AssertObjectGraphDoesNotContainSymbolsOrCompilationData(object obj)
        {
            var visited = new HashSet<object>();
            Visit(obj);

            void Visit(object? node)
            {
                if (node is null || !visited.Add(node))
                {
                    return;
                }

                Assert.False(node is Compilation or ISymbol or SyntaxNode);

                var type = node.GetType();
                if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                {
                    return;
                }

                if (node is IEnumerable collection and not string)
                {
                    foreach (var element in collection)
                    {
                        Visit(element);
                    }

                    return;
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var fieldValue = field.GetValue(node);
                    Visit(fieldValue);
                }
            }
        }
    }

    private class ChangeCultureScope : IDisposable
    {
        private readonly CultureInfo _originalCurrentCulture;
        private readonly CultureInfo _originalCurrentUiCulture;

        public ChangeCultureScope(string cultureName)
        {
            _originalCurrentCulture = CultureInfo.CurrentCulture;
            _originalCurrentUiCulture = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCurrentCulture;
            CultureInfo.CurrentUICulture = _originalCurrentUiCulture;
        }
    }
}
