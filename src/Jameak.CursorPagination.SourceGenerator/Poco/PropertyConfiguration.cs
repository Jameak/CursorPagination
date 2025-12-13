using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.SourceGenerator.Poco;

internal record PropertyConfiguration
{
    public int Order { get; }
    public string PropertyAccessor { get; }
    public string PropertyTypeFullName { get; private set; }
    public PaginationOrdering Direction { get; }
    public string? NullCoalesceRhs { get; }
    public bool IsNullableValueType { get; }

    public PropertyConfiguration(
        int order,
        string propertyAccessor,
        PaginationOrdering direction,
        string propertyTypeFullName,
        string? nullCoalesceRhs,
        bool isNullableValueType)
    {
        Order = order;
        PropertyAccessor = propertyAccessor;
        Direction = direction;
        PropertyTypeFullName = propertyTypeFullName;
        NullCoalesceRhs = nullCoalesceRhs;
        IsNullableValueType = isNullableValueType;
    }

    public string PropertyNameForCursorField => PropertyAccessor.Contains('.')
        ? PropertyAccessor.Replace('.', '_').Replace('?', '_').Replace('!', '_').Replace('@', '_')
        : PropertyAccessor;
}
