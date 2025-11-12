using System.Collections.Immutable;
using Jameak.CursorPagination.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using static Jameak.CursorPagination.SourceGenerator.DiagnosticHelper;

namespace Jameak.CursorPagination.SourceGenerator.Poco;

internal record CacheableDiagnosticInfo
{
    private readonly string _id;
    private readonly EquatableArray<CacheableLocation>? _locations;
    private readonly EquatableArray<string> _messageArgs;

    public CacheableDiagnosticInfo(string id, EquatableArray<CacheableLocation>? locations, string[] messageArgs)
    {
        _id = id;
        _locations = locations;
        _messageArgs = messageArgs.ToImmutableArray().AsEquatableArray();
    }

    public Diagnostic CreateDiagnostic()
    {
        var convertedLocations = _locations?.Select(loc => loc.RecreateLocation()).Where(e => e != null).Select(e => e!).ToArray();
        var firstLocation = convertedLocations.FirstOrDefault();
        var rest = convertedLocations.Skip(1);
        return Diagnostic.Create(FindById(_id), firstLocation, rest, _messageArgs.ToArray());
    }
}
