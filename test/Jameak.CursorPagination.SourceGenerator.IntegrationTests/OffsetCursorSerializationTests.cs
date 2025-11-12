using CsCheck;
using Jameak.CursorPagination.Abstractions.OffsetPagination;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests;
public class OffsetCursorSerializationTests
{
    [Fact]
    public void VerifyManualRoundtrip()
    {
        // Arrange
        var cursor = new OffsetCursor(10);

        // Act
        var cursorString = cursor.CursorToString();
        var roundtrippedCursor = OffsetCursor.CursorFromString(cursorString);

        // Assert
        Assert.Equal(cursor, roundtrippedCursor);
        Assert.Equal("MTA", cursorString);
    }

    [Fact]
    public void VerifyNegativeSkipValueThrows()
    {
        Assert.Throws<ArgumentException>(() => new OffsetCursor(-1));
    }

    [Fact]
    public void VerifyCursorRoundtrips()
    {
        var skipValGenerator = Gen.Int.Where(num => num >= 0);
        skipValGenerator.Sample(skipVal =>
        {
            var cursor = new OffsetCursor(skipVal);
            var serializedValue = cursor.CursorToString();
            var unserializedCursor = OffsetCursor.CursorFromString(serializedValue);
            return cursor.Equals(unserializedCursor);
        }, iter: 100_000);
    }
}
