using System.Linq.Expressions;
using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Abstractions.Attributes;

/// <summary>
/// Place this attribute on a class that also has the <see cref="KeySetPaginationStrategyAttribute"/>
/// or <see cref="OffsetPaginationStrategyAttribute"/> attribute to configure the pagination for a specific property.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PaginationPropertyAttribute : Attribute
{
    /// <summary>
    /// Attribute constructor with configuration arguments for source generation.
    /// </summary>
    /// <param name="Order">The order of this <paramref name="Property"/> compared to the other configured
    /// properties on the pagination strategy. Sorting happens from lowest-to-highest.</param>
    /// <param name="Property">The name of the property to add to the source generated pagination strategy.
    /// It is recommended to use <see langword="nameof"/> to specify this name.</param>
    /// <param name="Direction">Configures the sort-order of this property.</param>
    /// <param name="NullCoalesceRhs">If the configured <paramref name="Property"/> is nullable,
    /// use this parameter to specify a value to null-coalesce the argument to.
    /// The evaluated type of this string must match the type of the <paramref name="Property"/> type,
    /// be valid as the right-hand side of an <see cref="Expression"/>, and any involved types must be fully qualified.</param>
    public PaginationPropertyAttribute(int Order, string Property, PaginationOrdering Direction = PaginationOrdering.Ascending, string? NullCoalesceRhs = null)
    {
        this.Order = Order;
        this.Property = Property;
        this.Direction = Direction;
        this.NullCoalesceRhs = NullCoalesceRhs;
    }

    internal int Order { get; }
    internal string Property { get; }
    internal PaginationOrdering Direction { get; }
    internal string? NullCoalesceRhs { get; }
}
