using System.Collections.Immutable;
using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.SourceGenerator.Extractors;
using Jameak.CursorPagination.SourceGenerator.Helpers;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator;

[Generator]
internal partial class PaginationGenerator : IIncrementalGenerator
{
    private static readonly string s_offsetPaginationStrategyAttributeFullName = typeof(OffsetPaginationStrategyAttribute).FullName;
    private static readonly string s_keysetPaginationStrategyAttributeFullName = typeof(KeySetPaginationStrategyAttribute).FullName;
    private static readonly ImmutableArray<BaseExtractor> s_extractors = [new KeySetExtractor(), new OffsetExtractor()];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var offsetDataForGeneration = context.SyntaxProvider
            .ForAttributeWithMetadataName(
               s_offsetPaginationStrategyAttributeFullName,
                predicate: static (s, _) => IsTargetForGeneration(s),
                transform: static (ctx, _) => ExtractDataForGeneration(ctx)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .WithTrackingName(TrackingNames.InitialOffsetExtraction);

        var keysetDataForPagination = context.SyntaxProvider
            .ForAttributeWithMetadataName(
               s_keysetPaginationStrategyAttributeFullName,
                predicate: static (s, _) => IsTargetForGeneration(s),
                transform: static (ctx, _) => ExtractDataForGeneration(ctx)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .WithTrackingName(TrackingNames.InitialKeySetExtraction);

        var offsetWithUniqueNames = offsetDataForGeneration.Collect().SelectMany(RemapWithUniqueNames).WithTrackingName(TrackingNames.OffsetWithRemappedNames);
        var keysetWithUniqueNames = keysetDataForPagination.Collect().SelectMany(RemapWithUniqueNames).WithTrackingName(TrackingNames.KeySetWithRemappedNames);

        context.RegisterSourceOutput(offsetWithUniqueNames, static (spc, source) => Execute(spc, source, "_O"));
        context.RegisterSourceOutput(keysetWithUniqueNames, static (spc, source) => Execute(spc, source, "_K"));
    }

    private static ImmutableArray<BaseExtractedData> RemapWithUniqueNames(ImmutableArray<BaseExtractedData> extractedDataArray, CancellationToken cancellationToken)
    {
        var usedNames = new HashSet<string>();
        var extractionsWithPotentiallyUpdatedNames = new List<BaseExtractedData>();
        foreach (var extractedData in extractedDataArray
            .OrderBy(e => e.Name, StringComparer.Ordinal)
            .ThenBy(e => e is ExtractedKeySetData kd ? kd.Namespace : (e is ExtractedOffsetData ed ? ed.Namespace : ""))
            .ThenBy(e => CreateMd5Hash(e.ToString())))
        {
            for (var i = -1; true; i++)
            {
                if (i == -1)
                {
                    if (usedNames.Add(extractedData.Name))
                    {
                        extractionsWithPotentiallyUpdatedNames.Add(extractedData);
                        break;
                    }
                }
                else
                {
                    var candidateName = extractedData.Name + i;
                    if (usedNames.Add(candidateName))
                    {
                        var updated = extractedData with { HintName = candidateName };
                        extractionsWithPotentiallyUpdatedNames.Add(updated);
                        break;
                    }
                }
            }
        }

        return extractionsWithPotentiallyUpdatedNames.ToImmutableArray().AsEquatableArray();
    }

    private static void Execute(SourceProductionContext context, BaseExtractedData extractedData, string hintNamePostfix)
    {
        ReportDiagnostics(context, extractedData.ExtractionWarnings);
        if (extractedData.ExtractionErrors.Any())
        {
            ReportDiagnostics(context, extractedData.ExtractionErrors);
            return;
        }

        (string fileContent, List<CacheableDiagnosticInfo> errors) result;
        try
        {
            switch (extractedData)
            {
                case ExtractedKeySetData extractedKeySetData:
                    result = KeySetPaginationClassBuilder.GenerateClass(extractedKeySetData);
                    break;
                case ExtractedOffsetData extractedOffsetData:
                    result = OffsetPaginationClassBuilder.GenerateClass(extractedOffsetData);
                    break;
                default:
                    // Code error. Should never happen.
                    ReportDiagnostics(context, [DiagnosticHelper.CreateCouldNotGenerateForTypeDiagnostic(extractedData.PaginationStrategyTypeLocations, extractedData.Name)]);
                    return;
            }
        }
        catch (Exception)
        {
            ReportDiagnostics(context, [DiagnosticHelper.CreateCouldNotGenerateForTypeDiagnostic(extractedData.PaginationStrategyTypeLocations, extractedData.Name)]);
            return;
        }

        if (result.errors.Any())
        {
            ReportDiagnostics(context, result.errors);
            return;
        }

        context.AddSource($"{extractedData.HintName}{hintNamePostfix}.g.cs", result.fileContent);

        static void ReportDiagnostics(SourceProductionContext context, IEnumerable<CacheableDiagnosticInfo> toReport)
        {
            foreach (var extractionDiagnostic in toReport)
            {
                context.ReportDiagnostic(extractionDiagnostic.CreateDiagnostic());
            }
        }
    }

    private static bool IsTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode.IsKind(SyntaxKind.ClassDeclaration)
        && syntaxNode is ClassDeclarationSyntax;

    private static BaseExtractedData? ExtractDataForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol generatorClassSymbol || generatorClassSymbol.TypeKind == TypeKind.Error)
        {
            // Type isn't available or has errors. We cant do anything.
            return null;
        }

        try
        {
            if (!((ClassDeclarationSyntax)context.TargetNode).Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                var locations = CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations);
                return new BaseExtractedData(
                                generatorClassSymbol.Name,
                                [DiagnosticHelper.CreateClassIsMissingPartialKeywordDiagnostic(locations, generatorClassSymbol.Name)],
                                [],
                                locations);
            }

            BaseExtractedData? extractedDataOutput = null;
            var foundExtractor = false;
            foreach (var extractor in s_extractors)
            {
                var canHandle = extractor.TryHandle(context, generatorClassSymbol, out var extractedData);

                if (canHandle)
                {
                    if (foundExtractor)
                    {
                        // Error if multiple extractors were found:
                        var locations = CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations);
                        return new BaseExtractedData(
                                generatorClassSymbol.Name,
                                [DiagnosticHelper.CreatePaginationStrategyCannotBeDeclaredWithBothPaginationAttributesDiagnostic(locations, generatorClassSymbol.Name)],
                                [],
                                locations);
                    }

                    foundExtractor = true;
                    extractedDataOutput = extractedData;
                }
            }

            if (extractedDataOutput == null)
            {
                return CreateUnspecificGenerationErrorResult(generatorClassSymbol);
            }

            return extractedDataOutput;
        }
        catch (Exception)
        {
            return CreateUnspecificGenerationErrorResult(generatorClassSymbol);
        }
    }
}
