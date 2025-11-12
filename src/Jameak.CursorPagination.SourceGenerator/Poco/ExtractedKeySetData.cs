using System.Collections.Immutable;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.SourceGenerator.Helpers;

namespace Jameak.CursorPagination.SourceGenerator.Poco;
internal record ExtractedKeySetData : BaseExtractedData
{
    public string? Namespace { get; }
    public string Visibility { get; }
    public string PaginationTargetTypeFullname { get; }
    public EquatableArray<PropertyConfiguration> PropertyConfigurations { get; }
    public KeySetCursorSerializerGeneration GenerateKeySetCursorSerialization { get; }

    public ExtractedKeySetData(
        string name,
        string hintName,
        string? @namespace,
        string visibility,
        string paginationTargetTypeFullname,
        EquatableArray<CacheableLocation> paginationStrategyTypeLocations,
        IEnumerable<PropertyConfiguration> propertyConfigurations,
        IEnumerable<CacheableDiagnosticInfo> extractionErrors,
        IEnumerable<CacheableDiagnosticInfo> extractionWarnings,
        KeySetCursorSerializerGeneration generateKeySetTokenSerialization) : base(name, hintName, extractionErrors, extractionWarnings, paginationStrategyTypeLocations)
    {
        Namespace = @namespace;
        Visibility = visibility;
        PaginationTargetTypeFullname = paginationTargetTypeFullname;
        PropertyConfigurations = propertyConfigurations.OrderBy(e => e.Order).ToImmutableArray().AsEquatableArray();
        GenerateKeySetCursorSerialization = generateKeySetTokenSerialization;
    }
}
