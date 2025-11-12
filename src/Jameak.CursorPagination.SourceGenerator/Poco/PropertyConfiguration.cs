using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.SourceGenerator.Poco;

internal record PropertyConfiguration
{
    public int Order { get; }
    public string PropertyName { get; }
    public string PropertyTypeFullName { get; private set; }
    public PaginationOrdering Direction { get; }
    public string? NullCoalesceRhs { get; }
    public bool IsNullableValueType { get; }

    public PropertyConfiguration(
        int order,
        string propertyName,
        PaginationOrdering direction,
        string propertyTypeFullName,
        string? nullCoalesceRhs,
        bool isNullableValueType)
    {
        Order = order;
        PropertyName = propertyName;
        Direction = direction;
        PropertyTypeFullName = propertyTypeFullName;
        NullCoalesceRhs = nullCoalesceRhs;
        IsNullableValueType = isNullableValueType;
    }
}
