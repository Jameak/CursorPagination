using Jameak.CursorPagination.SourceGenerator.Poco;
using Microsoft.CodeAnalysis;
using static Jameak.CursorPagination.SourceGenerator.HelperMethods;

namespace Jameak.CursorPagination.SourceGenerator.Extractors;
internal abstract partial class BaseExtractor
{
    internal abstract bool TryHandle(GeneratorAttributeSyntaxContext context, INamedTypeSymbol generatorClassSymbol, out BaseExtractedData? extractedData);

    protected static bool HasGeneralErrors(INamedTypeSymbol generatorClassSymbol, ITypeSymbol paginationTargetType, out BaseExtractedData? extractedData)
    {
        extractedData = null;
        if (paginationTargetType is not INamedTypeSymbol paginationNamedTargetType)
        {
            extractedData = CreateError(
                generatorClassSymbol,
                DiagnosticHelper.CreateGeneralPaginatedTypeIsUnsupportedDiagnostic(CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations), paginationTargetType.ToNameWithGenerics()));
            return true;
        }

        if (generatorClassSymbol.ContainingType != null)
        {
            var locations = CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations);
            extractedData = new BaseExtractedData(
                            generatorClassSymbol.Name,
                            [DiagnosticHelper.CreateNestedClassIsNotSupportedDiagnostic(locations, generatorClassSymbol.Name)],
                            [],
                            locations);
            return true;
        }

        if (IsErrorKind(paginationNamedTargetType))
        {
            extractedData = CreateError(
                generatorClassSymbol,
                DiagnosticHelper.CreateRequiredTypeIsErrorKindDiagnostic(CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations), paginationNamedTargetType.ToNameWithGenerics()));
            return true;
        }

        if (paginationNamedTargetType.IsUnboundGenericType)
        {
            extractedData = CreateError(
                generatorClassSymbol,
                DiagnosticHelper.CreateUnboundGenericIsNotSupportedDiagnostic(CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations), generatorClassSymbol.Name, paginationNamedTargetType.ToNameWithGenerics()));
            return true;
        }

        if (generatorClassSymbol.IsGenericType)
        {
            extractedData = CreateError(
                generatorClassSymbol,
                DiagnosticHelper.CreateGenericClassIsNotSupportedDiagnostic(CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations), generatorClassSymbol.ToNameWithGenerics()));
            return true;
        }

        return false;

        static BaseExtractedData CreateError(INamedTypeSymbol generatorClassSymbol, CacheableDiagnosticInfo diagnostic)
        {
            return new BaseExtractedData(
                generatorClassSymbol.Name,
                [diagnostic],
                [],
                CacheableLocation.CreateFromLocations(generatorClassSymbol.Locations));
        }
    }

    protected static (List<PropertyConfiguration> properties, IReadOnlyList<CacheableDiagnosticInfo> errors, IReadOnlyList<CacheableDiagnosticInfo> warnings)
        ExtractPaginationProperties(
            GeneratorAttributeSyntaxContext context,
            INamedTypeSymbol generatorClassSymbol,
            ITypeSymbol paginationTargetType,
            PaginationKind paginationKind)
    {
        var propertyExtractor = new PaginationPropertyExtractionHelper(context, generatorClassSymbol, paginationTargetType, paginationKind);
        return propertyExtractor.Extract();
    }
}
