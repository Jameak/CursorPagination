using Jameak.CursorPagination.Abstractions.Attributes;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator.Extractors;
internal class KeySetExtractor : BaseExtractor
{
    internal override bool TryHandle(GeneratorAttributeSyntaxContext context, INamedTypeSymbol generatorClassSymbol, out BaseExtractedData? extractedData)
    {
        extractedData = null;
        ITypeSymbol? paginationTargetType = null;
        KeySetCursorSerializerGeneration? generateKeySetCursorSerialization = null;

        foreach (var attrData in generatorClassSymbol.GetAttributes().FilterWithAttributeType<KeySetPaginationStrategyAttribute>())
        {
            ExtractKeySetAttributeData(attrData, ref paginationTargetType, ref generateKeySetCursorSerialization);
        }

        if (paginationTargetType == null || !generateKeySetCursorSerialization.HasValue)
        {
            return false;
        }

        if (HasGeneralErrors(generatorClassSymbol, paginationTargetType, out var extractedErrors))
        {
            extractedData = extractedErrors;
            return true;
        }

        extractedData = ExtractKeySetGenerationData(
            context,
            generatorClassSymbol,
            (INamedTypeSymbol)paginationTargetType,
            generateKeySetCursorSerialization.Value);
        return true;
    }

    private static void ExtractKeySetAttributeData(
        AttributeData paginationStrategyAttributeData,
        ref ITypeSymbol? paginationTargetType,
        ref KeySetCursorSerializerGeneration? generateCursorSerialization)
    {
        var constructorArgs = paginationStrategyAttributeData.ConstructorArguments;
        if (HasErrors(constructorArgs))
        {
            return;
        }

        // Assumes KeySetPaginationStrategyAttribute only has a single constructor.
        paginationTargetType = GetArgumentValue(constructorArgs[0]) as ITypeSymbol;
        generateCursorSerialization = GetArgumentValue(constructorArgs[1], typeof(KeySetCursorSerializerGeneration)) as KeySetCursorSerializerGeneration?;
    }

    private static BaseExtractedData? ExtractKeySetGenerationData(
        GeneratorAttributeSyntaxContext context,
        INamedTypeSymbol generatorClassSymbol,
        ITypeSymbol paginationTargetType,
        KeySetCursorSerializerGeneration generateKeySetCursorSerialization)
    {
        var propExtraction = ExtractPaginationProperties(context, generatorClassSymbol, paginationTargetType, PaginationKind.KeySet);

        if (propExtraction.errors.Count != 0)
        {
            return new BaseExtractedData(generatorClassSymbol.Name, propExtraction.errors, propExtraction.warnings, CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations));
        }

        if (propExtraction.properties.Count > 0)
        {
            return new ExtractedKeySetData(
                generatorClassSymbol.ToNameWithGenericsAndEscapedKeywords(),
                SanitizeToValidFilename(generatorClassSymbol.Name),
                GetEnclosingNamespace(generatorClassSymbol),
                GetAccessibility(generatorClassSymbol),
                paginationTargetType.ToFullyQualified(),
                CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations),
                propExtraction.properties,
                [],
                propExtraction.warnings,
                generateKeySetCursorSerialization);
        }

        return CreateUnspecificGenerationErrorResult(generatorClassSymbol);
    }
}
