namespace Jameak.CursorPagination.Abstractions.Attributes;

/// <summary>
/// Place this attribute on a class to source generate a Offset pagination strategy with the specified configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OffsetPaginationStrategyAttribute : Attribute
{
    /// <summary>
    /// Attribute constructor with configuration arguments for source generation.
    /// </summary>
    /// <param name="Type">The type to generate a pagination strategy for.</param>
    public OffsetPaginationStrategyAttribute(Type Type)
    {
        this.Type = Type;
    }

    internal Type Type { get; }
}
