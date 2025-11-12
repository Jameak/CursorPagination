using System.Collections.Immutable;
using Jameak.CursorPagination.SourceGenerator.Helpers;

namespace Jameak.CursorPagination.SourceGenerator.Poco;
internal record ExtractedOffsetData : BaseExtractedData
{
    public string? Namespace { get; }
    public string Visibility { get; }
    public string PaginationTargetTypeFullname { get; }
    public EquatableArray<PropertyConfiguration> PropertyConfigurations { get; }

    public ExtractedOffsetData(
        string name,
        string hintName,
        string? @namespace,
        string visibility,
        string paginationTargetTypeFullname,
        EquatableArray<CacheableLocation> paginationStrategyTypeLocations,
        IEnumerable<PropertyConfiguration> propertyConfigurations,
        IEnumerable<CacheableDiagnosticInfo> extractionErrors,
        IEnumerable<CacheableDiagnosticInfo> extractionWarnings) : base(name, hintName, extractionErrors, extractionWarnings, paginationStrategyTypeLocations)
    {
        Visibility = visibility;
        PaginationTargetTypeFullname = paginationTargetTypeFullname;
        PropertyConfigurations = propertyConfigurations.OrderBy(e => e.Order).ToImmutableArray().AsEquatableArray();
        Namespace = @namespace;
    }
}
