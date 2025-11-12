using Jameak.CursorPagination.Abstractions.Enums;

namespace Jameak.CursorPagination.Abstractions.Attributes;

/// <summary>
/// Place this attribute on a class to source generate a KeySet pagination strategy with the specified configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class KeySetPaginationStrategyAttribute : Attribute
{
    /// <summary>
    /// Attribute constructor with configuration arguments for source generation.
    /// </summary>
    /// <param name="Type">The type to generate a pagination strategy for.</param>
    /// <param name="CursorSerialization">Controls whether KeySet cursor serialization code will be source generated alongside the pagination strategy.</param>
    public KeySetPaginationStrategyAttribute(Type Type, KeySetCursorSerializerGeneration CursorSerialization)
    {
        this.Type = Type;
        this.CursorSerialization = CursorSerialization;
    }

    internal Type Type { get; }
    internal KeySetCursorSerializerGeneration CursorSerialization { get; }
}
