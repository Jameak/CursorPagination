using CsCheck;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;
using Jameak.CursorPagination.SourceGenerator.IntegrationTests.PartialStrategies;

namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests;
public class KeySetCursorSerializationTests
{
    [Fact]
    public void VerifyCursorSerializationManyTypes()
    {
        // Arrange
        var input = new WideTypeVarietyPoco()
        {
            BoolProp = false,
            ByteProp = 2,
            DateTimeOffsetProp = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 6, TimeSpan.Zero),
            DateTimeProp = new DateTime(2025, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc),
            NullableDateTimeProp = null,
            DecimalProp = 3,
            DoubleProp = 4.1,
            FloatProp = 5.2f,
            GuidProp = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            IntProp = -6,
            LongProp = -7,
            SbyteProp = 8,
            ShortProp = 9,
            StringProp = "abc",
            UintProp = 10,
            UlongProp = 11,
            UshortProp = 12,
        };

        var strategy = new KeySetStrategyWithWideTypeVariety();

        // Act
        var cursor = strategy.CreateCursor(input);
        var cursorString = strategy.CursorToString(cursor);
        var deserializedCursor = strategy.CursorFromString(cursorString);

        // Assert
        Assert.Equal("eyIwIjpmYWxzZSwiMSI6MiwiMiI6IjYzODcxMzgzODQ1MDA2MDAwMDpVVEMiLCIzIjoiMjAyNC0wMS0wMlQwMzowNDowNS4wMDYrMDA6MDAiLCI0IjozLCI1Ijo0LjEsIjYiOjUuMiwiNyI6ImFhYWFhYWFhLWFhYWEtYWFhYS1hYWFhLWFhYWFhYWFhYWFhYSIsIjgiOi02LCI5IjotNywiMTAiOjgsIjExIjo5LCIxMiI6ImFiYyIsIjEzIjoxMiwiMTQiOjEwLCIxNSI6MTEsIjE2IjpudWxsfQ",
            cursorString);
        Assert.Equal(cursor, deserializedCursor);
    }

    [Fact]
    public void VerifyCursorSerializationPropertiesWithSameType()
    {
        var input = new SimplePropertyPoco()
        {
            IntProp = 1,
            StringProp1 = "abc",
            StringProp2 = "def"
        };
        var strategy = new KeySetStrategyAllAscending();

        // Act
        var cursor = strategy.CreateCursor(input);
        var cursorString = strategy.CursorToString(cursor);
        var deserializedCursor = strategy.CursorFromString(cursorString);

        // Assert
        Assert.Equal("eyIwIjoxLCIxIjoiYWJjIiwiMiI6ImRlZiJ9", cursorString);
        Assert.Equal(cursor, deserializedCursor);
    }

    [Fact]
    public void VerifyCursorRoundtripsWideTypeVariety()
    {
        var strategy = new KeySetStrategyWithWideTypeVariety();
        var cursorGen = new CursorGenerator();
        cursorGen.Sample(cursor =>
        {
            var serializedValue = strategy.CursorToString(cursor);
            var unserializedCursor = strategy.CursorFromString(serializedValue);
            return cursor.Equals(unserializedCursor);
        }, iter: 100_000);
    }

    private sealed class CursorGenerator : Gen<KeySetStrategyWithWideTypeVariety.Cursor>
    {
        private readonly Gen<long> _longGen = Gen.Long;
        private readonly Gen<bool> _boolGen = Gen.Bool;
        private readonly Gen<byte> _byteGen = Gen.Byte;
        private readonly Gen<DateTime> _dateTimeGen = Gen.OneOf(Gen.DateTime, Gen.DateTime.Utc, Gen.DateTime.Local);
        private readonly Gen<DateTimeOffset> _dateTimeOffsetGen = Gen.DateTimeOffset;
        private readonly Gen<decimal> _decimalGen = Gen.Decimal;
        private readonly Gen<double> _doubleGen = Gen.OneOf(Gen.Double, Gen.Double.Special);
        private readonly Gen<float> _floatGen = Gen.OneOf(Gen.Float, Gen.Float.Special);
        private readonly Gen<Guid> _guidGen = Gen.Guid;
        private readonly Gen<int> _intGen = Gen.Int;
        private readonly Gen<sbyte> _sbyteGen = Gen.SByte;
        private readonly Gen<short> _shortGen = Gen.Short;
        private readonly Gen<string> _stringGen = Gen.String;
        private readonly Gen<ushort> _ushortGen = Gen.UShort;
        private readonly Gen<uint> _uintGen = Gen.UInt;
        private readonly Gen<ulong> _ulongGen = Gen.ULong;

        public override KeySetStrategyWithWideTypeVariety.Cursor Generate(PCG pcg, Size? min, out Size size)
        {
            var cursor = new KeySetStrategyWithWideTypeVariety.Cursor(
                _boolGen.Generate(pcg, min, out var sizeBool),
                _byteGen.Generate(pcg, min, out var sizeByte),
                _dateTimeGen.Generate(pcg, min, out var sizeDateTime),
                _dateTimeOffsetGen.Generate(pcg, min, out var sizeDateTimeOffset),
                _decimalGen.Generate(pcg, min, out var sizeDecimal),
                _doubleGen.Generate(pcg, min, out var sizeDouble),
                _floatGen.Generate(pcg, min, out var sizeFloat),
                _guidGen.Generate(pcg, min, out var sizeGuid),
                _intGen.Generate(pcg, min, out var sizeInt),
                _longGen.Generate(pcg, min, out var sizeLong),
                _sbyteGen.Generate(pcg, min, out var sizeSbyte),
                _shortGen.Generate(pcg, min, out var sizeShort),
                _stringGen.Generate(pcg, min, out var sizeString),
                _ushortGen.Generate(pcg, min, out var sizeUshort),
                _uintGen.Generate(pcg, min, out var sizeUint),
                _ulongGen.Generate(pcg, min, out var sizeUlong),
                _boolGen.Generate(pcg, min, out var sizeNullableDateTime) ? _dateTimeGen.Generate(pcg, min, out sizeNullableDateTime) : null);
            size = sizeBool;
            size.Add(sizeByte);
            size.Add(sizeDateTime);
            size.Add(sizeDateTimeOffset);
            size.Add(sizeDecimal);
            size.Add(sizeDouble);
            size.Add(sizeFloat);
            size.Add(sizeGuid);
            size.Add(sizeInt);
            size.Add(sizeLong);
            size.Add(sizeSbyte);
            size.Add(sizeShort);
            size.Add(sizeString);
            size.Add(sizeUshort);
            size.Add(sizeUint);
            size.Add(sizeUlong);
            size.Add(sizeNullableDateTime);
            return cursor;
        }
    }
}
