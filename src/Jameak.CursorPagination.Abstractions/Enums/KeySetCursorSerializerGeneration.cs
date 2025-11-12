using Jameak.CursorPagination.Abstractions.KeySetPagination;

namespace Jameak.CursorPagination.Abstractions.Enums;
/// <summary>
/// The possible KeySet cursor serializer implementations that can be source generated.
/// </summary>
public enum KeySetCursorSerializerGeneration
{
    /// <summary>
    /// Opt-out of source generated KeySet cursor serialization.
    /// </summary>
    /// <remarks>
    /// <para>This is mostly useful if your paginated types have special requirements for serialization,
    /// as you can then implement the <see cref="IKeySetCursorSerializer{TCursor}"/> interface for
    /// the source generated <see cref="IKeySetCursor"/>-type manually.</para>
    /// </remarks>
    DoNotGenerate = 1,
    /// <summary>
    /// Opt-in to source generated KeySet cursor serialization implemented via System.Text.Json.
    /// </summary>
    /// <remarks>
    /// The source-generated implementation uses a <b>JsonNamingPolicy</b> to hide the paginated cursors property names.
    /// </remarks>
    UseSystemTextJson = 2
}
