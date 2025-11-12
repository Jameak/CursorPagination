namespace Jameak.CursorPagination.SourceGenerator.IntegrationTests.InputClasses;

internal record WideTypeVarietyPoco
{
    public required bool BoolProp { get; set; }
    public required byte ByteProp { get; set; }
    public required DateTime DateTimeProp { get; set; }
    public required DateTime? NullableDateTimeProp { get; set; }
    public required DateTimeOffset DateTimeOffsetProp { get; set; }
    public required decimal DecimalProp { get; set; }
    public required double DoubleProp { get; set; }
    public required float FloatProp { get; set; }
    public required Guid GuidProp { get; set; }
    public required int IntProp { get; set; }
    public required long LongProp { get; set; }
    public required sbyte SbyteProp { get; set; }
    public required short ShortProp { get; set; }
    public required string StringProp { get; set; }
    public required ushort UshortProp { get; set; }
    public required uint UintProp { get; set; }
    public required ulong UlongProp { get; set; }
}
