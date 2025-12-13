using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Basic.Reference.Assemblies;
using Jameak.CursorPagination.Abstractions.KeySetPagination;
using Jameak.CursorPagination.SourceGenerator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jameak.CursorPagination.SourceGenerator.Tests;

public static class TestHelper
{
    private static readonly ImmutableArray<DiagnosticAnalyzer> s_analyzers = [new InternalUsageDiagnosticAnalyzer(), new FullNameOfDiagnosticAnalyzer()];

    private static readonly HashSet<string> s_allTrackingNames = typeof(TrackingNames)
        .GetFields()
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        .Select(x => (string)x.GetRawConstantValue()!)
        .Where(x => !string.IsNullOrEmpty(x))
        .Select(e => e!)
        .ToHashSet();

    private static readonly IReadOnlyList<PortableExecutableReference> s_references = [
            .. Net80.References.All,
            // Jameak.CursorPagination.Abstractions
            MetadataReference.CreateFromFile(typeof(IKeySetCursor).Assembly.Location)
        ];

    public static async Task AssertDiagnosticsWithCodeFixer<T>(
        string sourceTextUnderTest,
        string expectedFix,
        List<string> otherSourceTexts,
        List<(string diagnosticId, int count)> expectedDiagnostics)
        where T : CodeFixProvider, new()
    {
        var (documentUnderTest, solution) = SetupTestProject(sourceTextUnderTest, otherSourceTexts);
        var analyzerDiagnostics = await RunProjectCompilationGetAnalyzerDiagnostics(documentUnderTest.Project);
        AssertAnalyzerDiagnosticsAsExpected(analyzerDiagnostics, expectedDiagnostics);
        await AssertCodeFixAsExpected(documentUnderTest, solution, expectedFix, analyzerDiagnostics);

        static async Task<ImmutableArray<Diagnostic>> RunProjectCompilationGetAnalyzerDiagnostics(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var analyzerCompilation = compilation!.WithAnalyzers(s_analyzers);
            var analyzerDiags = await analyzerCompilation.GetAnalyzerDiagnosticsAsync(CancellationToken.None);
            return analyzerDiags;
        }

        static (Document documentUnderTest, Solution solution) SetupTestProject(
            string sourceTextUnderTest,
            List<string> otherSourceTexts)
        {
            var testProjectName = "TestProject";
            var projectId = ProjectId.CreateNewId(debugName: testProjectName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, testProjectName, testProjectName, LanguageNames.CSharp)
                .WithProjectParseOptions(projectId, CSharpParseOptions.Default)
                .AddMetadataReferences(projectId, s_references);

            for (var i = 0; i < otherSourceTexts.Count; i++)
            {
                var source = otherSourceTexts[i];
                var fileName = "Test" + i.ToString(CultureInfo.InvariantCulture) + ".cs";
                var documentId = DocumentId.CreateNewId(projectId, debugName: fileName);
                solution = solution.AddDocument(documentId, fileName, SourceText.From(source), filePath: fileName);
            }

            var documentUnderTestId = DocumentId.CreateNewId(projectId, debugName: "UnderTest.cs");
            solution = solution.AddDocument(documentUnderTestId, "UnderTest.cs", SourceText.From(sourceTextUnderTest), filePath: "UnderTest.cs");

            var project = solution.GetProject(projectId)!;
            var document = project.GetDocument(documentUnderTestId)!;
            return (document, solution);
        }

        static void AssertAnalyzerDiagnosticsAsExpected(
            ImmutableArray<Diagnostic> analyzerDiagnostics,
            List<(string diagnosticId, int count)> expectedDiagnostics)
        {
            var expectedMap = expectedDiagnostics.ToDictionary(e => e.diagnosticId, e => e.count);
            foreach (var diagnosticsGroup in analyzerDiagnostics.GroupBy(e => e.Id))
            {
                var wasExpected = expectedMap.TryGetValue(diagnosticsGroup.Key, out var expectedCount);
                Assert.True(wasExpected);
                Assert.Equal(expectedCount, diagnosticsGroup.Count());
            }
        }

        static async Task AssertCodeFixAsExpected(
            Document documentUnderTest,
            Solution solution,
            string expectedFix,
            ImmutableArray<Diagnostic> analyzerDiagnostics)
        {
            var updatedSolution = solution;
            var updatedDocument = documentUnderTest;
            var codeFixer = new T();
            foreach (var diagnostic in analyzerDiagnostics)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(updatedDocument, diagnostic, (a, _) => actions.Add(a), CancellationToken.None);
                await codeFixer.RegisterCodeFixesAsync(context);

                if (actions.Count == 0)
                {
                    continue;
                }

                var operations = await actions.Single().GetOperationsAsync(CancellationToken.None);
                updatedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
                updatedDocument = updatedSolution.GetDocument(documentUnderTest.Id)!;
            }

            var documentString = await updatedDocument.GetTextAsync();
            Assert.Equal(expectedFix, documentString.ToString(), ignoreLineEndingDifferences: true);
        }
    }

    private static async Task<(GeneratorDriver driverWithResults, CSharpCompilation compilation, ImmutableArray<Diagnostic> analyzerDiagnostics)> RunSourceGenerationTestCompilation(IEnumerable<string> sourceText)
    {
        var compileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithSpecificDiagnosticOptions(s_analyzers.SelectMany(e => e.SupportedDiagnostics).Select(diag => new KeyValuePair<string, ReportDiagnostic>(diag.Id, GetReportDiagnostic(diag))));
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: sourceText.Select(sourceCode => CSharpSyntaxTree.ParseText(sourceCode)),
            references: s_references,
            compileOptions);
        var generator = new PaginationGenerator();
        var generatorDriverOptions = new GeneratorDriverOptions(disabledOutputs: IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true);
        var driver = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], driverOptions: generatorDriverOptions);
        var updatedDriverWithResults = driver.RunGenerators(compilation);

        AssertGeneratorOutputIsCacheable(compilation, updatedDriverWithResults);

        var analyzerCompilation = compilation.WithAnalyzers(s_analyzers);
        var analyzerDiags = await analyzerCompilation.GetAnalyzerDiagnosticsAsync(CancellationToken.None);
        return (updatedDriverWithResults, compilation, analyzerDiags);
    }

    public static Task VerifySourceGeneration([StringSyntax("csharp")] string sourceCode, bool diagnosticsOnly = false, [CallerFilePath] string callerFilePath = "")
    {
        return VerifySourceGeneration([sourceCode], diagnosticsOnly, callerFilePath);
    }

    public static async Task VerifySourceGeneration(IEnumerable<string> sourceText, bool diagnosticsOnly = false, [CallerFilePath] string callerFilePath = "")
    {
        var settings = new VerifySettings();
        settings.UseDirectory(Path.Combine(Path.GetDirectoryName(callerFilePath)!, "__snapshots__"));
        // Compilation errors are localized, so to ensure snapshot reproducibility we force a consistent culture.
        using var cultureScope = new ChangeCultureScope("en-US");

        var (driverWithResults, compilation, analyzerDiagnostics) = await RunSourceGenerationTestCompilation(sourceText);

        // Generator reported error diagnostics
        if (driverWithResults.GetRunResult().Results
            .SelectMany(e => e.Diagnostics.Where(e => e.DefaultSeverity == DiagnosticSeverity.Error))
            .Any())
        {
            await Verifier.Verify(driverWithResults, settings)
                .AppendAnalyzerDiagsIfAny(analyzerDiagnostics);
            return;
        }

        // No error diagnostics reported.
        // Compile the generated code to check that the emitted code is actually valid
        var updatedCompilation = compilation.AddSyntaxTrees(
            driverWithResults.GetRunResult().Results
            .SelectMany(r => r.GeneratedSources)
            .Select(gs => CSharpSyntaxTree.ParseText(gs.SourceText, CSharpParseOptions.Default, gs.HintName)));

        using var dll = new MemoryStream();
        var emitted = updatedCompilation.Emit(dll);


        if (diagnosticsOnly)
        {
            await Verifier.Verify(analyzerDiagnostics
                .Concat(driverWithResults.GetRunResult().Results.SelectMany(e => e.Diagnostics))
                .Concat(emitted.Diagnostics));
            return;
        }

        await Verifier.Verify(driverWithResults, settings)
            .AppendValue("generated-code-can-compile", emitted.Success)
            .AppendValue("generated-code-compilation-diagnostics", emitted.Diagnostics)
            .AppendAnalyzerDiagsIfAny(analyzerDiagnostics);
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
