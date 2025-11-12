using System.Collections.Immutable;
using Jameak.CursorPagination.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jameak.CursorPagination.SourceGenerator.Poco;

internal record CacheableLocation
{
    private readonly string? _filePath;
    private readonly TextSpan _sourceSpan;
    private readonly LinePositionSpan _lineSpan;

    public CacheableLocation(Location location)
    {
        _filePath = location.SourceTree?.FilePath;
        _sourceSpan = location.SourceSpan;
        _lineSpan = location.GetLineSpan().Span;
    }

    public Location? RecreateLocation()
    {
        return _filePath == null ? null : Location.Create(_filePath, _sourceSpan, _lineSpan);
    }

    public static EquatableArray<CacheableLocation> CreateFromLocations(IEnumerable<Location> locations)
    {
        return locations.Select(e => new CacheableLocation(e)).ToImmutableArray().AsEquatableArray();
    }
}
