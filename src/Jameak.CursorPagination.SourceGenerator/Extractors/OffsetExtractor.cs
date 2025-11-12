using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator.Extractors;
internal class OffsetExtractor : BaseExtractor
{
    internal override bool TryHandle(GeneratorAttributeSyntaxContext context, INamedTypeSymbol generatorClassSymbol, out BaseExtractedData? extractedData)
    {
        extractedData = null;
        INamedTypeSymbol? paginationTargetType = null;

        foreach (var attrData in generatorClassSymbol.GetAttributes().FilterWithAttributeType<OffsetPaginationStrategyAttribute>())
        {
            ExtractOffsetAttributeData(attrData, ref paginationTargetType);
        }

        if (paginationTargetType == null)
        {
            return false;
        }

        if (HasGeneralErrors(generatorClassSymbol, paginationTargetType, out var extractedErrors))
        {
            extractedData = extractedErrors;
            return true;
        }

        extractedData = ExtractOffsetGenerationData(context, generatorClassSymbol, paginationTargetType);
        return true;
    }

    private static void ExtractOffsetAttributeData(
        AttributeData paginationStrategyAttributeData,
        ref INamedTypeSymbol? paginationTargetType)
    {
        var constructorArgs = paginationStrategyAttributeData.ConstructorArguments;
        if (HasErrors(constructorArgs))
        {
            return;
        }

        // Assumes OffsetPaginationStrategyAttribute only has a single constructor.
        paginationTargetType = GetArgumentValue(constructorArgs[0]) as INamedTypeSymbol;
    }

    private static BaseExtractedData? ExtractOffsetGenerationData(
        GeneratorAttributeSyntaxContext context,
        INamedTypeSymbol generatorClassSymbol,
        ITypeSymbol paginationTargetType)
    {
        var propExtraction = ExtractPaginationProperties(context, generatorClassSymbol, paginationTargetType, PaginationKind.Offset);

        if (propExtraction.errors.Count != 0)
        {
            return new BaseExtractedData(generatorClassSymbol.Name, propExtraction.errors, propExtraction.warnings, CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations));
        }

        if (propExtraction.properties.Count > 0)
        {
            return new ExtractedOffsetData(
                generatorClassSymbol.Name,
                SanitizeToValidFilename(generatorClassSymbol.Name),
                GetEnclosingNamespace(generatorClassSymbol),
                GetAccessibility(generatorClassSymbol),
                paginationTargetType.ToFullyQualified(),
                CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations),
                propExtraction.properties,
                [],
                propExtraction.warnings);
        }

        return CreateUnspecificGenerationErrorResult(generatorClassSymbol);
    }
}
