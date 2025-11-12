using System.Collections.Immutable;
using Jameak.CursorPagination.SourceGenerator.Helpers;

namespace Jameak.CursorPagination.SourceGenerator.Poco;
internal record BaseExtractedData
{
    public string Name { get; }
    public string HintName { get; set; }
    public EquatableArray<CacheableDiagnosticInfo> ExtractionErrors { get; }
    public EquatableArray<CacheableDiagnosticInfo> ExtractionWarnings { get; }
    public EquatableArray<CacheableLocation> PaginationStrategyTypeLocations { get; }

    public BaseExtractedData(
        string name,
        IEnumerable<CacheableDiagnosticInfo> extractionErrors,
        IEnumerable<CacheableDiagnosticInfo> extractionWarnings,
        EquatableArray<CacheableLocation> paginationStrategyTypeLocations)
    {
        Name = name;
        HintName = name;
        ExtractionErrors = extractionErrors.ToImmutableArray().AsEquatableArray();
        ExtractionWarnings = extractionWarnings.ToImmutableArray().AsEquatableArray();
        PaginationStrategyTypeLocations = paginationStrategyTypeLocations;
    }

    protected BaseExtractedData(
        string name,
        string hintName,
        IEnumerable<CacheableDiagnosticInfo> extractionErrors,
        IEnumerable<CacheableDiagnosticInfo> extractionWarnings,
        EquatableArray<CacheableLocation> paginationStrategyTypeLocations)
    {
        Name = name;
        HintName = hintName;
        ExtractionErrors = extractionErrors.ToImmutableArray().AsEquatableArray();
        ExtractionWarnings = extractionWarnings.ToImmutableArray().AsEquatableArray();
        PaginationStrategyTypeLocations = paginationStrategyTypeLocations;
    }
}
