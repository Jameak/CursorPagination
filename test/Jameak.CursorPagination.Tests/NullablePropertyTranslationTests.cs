using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Abstractions.Exceptions;
using Jameak.CursorPagination.Tests.DbTests;
using Jameak.CursorPagination.Tests.InputClasses;

namespace Jameak.CursorPagination.Tests;
public class NullablePropertyTranslationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFactory;

    public NullablePropertyTranslationTests(DatabaseFixture databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    [Fact]
    public async Task DbComputedNullableProperty_EfCanTranslate()
    {
        var strategy = new NullablePropertyWithDbComputedColumnPocoKeySetStrategy();
        var dbContext = _databaseFactory.CreateDbContext();
        var queryable = TestHelper.TagTestQueryable(dbContext.ComputedNullableTestTable);

        // Act
        var paginatedQueryable = strategy.ApplyPagination(queryable, 10, false, PaginationDirection.Forward, new NullablePropertyWithDbComputedColumnPocoKeySetStrategy.Cursor(10));
        var materialized = paginatedQueryable.ToList();

        // Assert
        await Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings()).AppendValue("db-ddl", _databaseFactory.DbCreateScript);
    }

    [Fact]
    public async Task NullablePropertyNoCoaslesce_EfCanTranslate()
    {
        var strategy = new NullablePropertyPocoKeySetStrategy();
        var dbContext = _databaseFactory.CreateDbContext();
        var queryable = TestHelper.TagTestQueryable(dbContext.NullableTestTable);

        // Act
        var paginatedQueryable = strategy.ApplyPagination(queryable, 10, false, PaginationDirection.Forward, new NullablePropertyPocoKeySetStrategy.Cursor(10, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), false, "abc"));
        var materialized = paginatedQueryable.ToList();

        // Assert
        await Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings());
    }

    [Fact]
    public async Task NullablePropertyWithCoaslesce_EfCanTranslate()
    {
        var strategy = new NullablePropertyPocoKeySetStrategyWithCoalesce();
        var dbContext = _databaseFactory.CreateDbContext();
        var queryable = TestHelper.TagTestQueryable(dbContext.NullableTestTable);

        // Act
        var paginatedQueryable = strategy.ApplyPagination(queryable, 10, false, PaginationDirection.Forward, new NullablePropertyPocoKeySetStrategyWithCoalesce.Cursor(10, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, "abc"));
        var materialized = paginatedQueryable.ToList();

        // Assert
        await Verify(TestHelper.TaggedLogMessages(dbContext), TestHelper.CreateVerifierSettings());
    }

    [Fact]
    public void NullablePropertyWithCoaslesce_NullCursorArgumentsDoNotThrow()
    {
        // Act
        var exception = Record.Exception(() => new NullablePropertyPocoKeySetStrategyWithCoalesce.Cursor(null, null, null, null));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void NullablePropertyNoCoaslesce_NullValueTypeCursorArgumentsThrowAndNonValueTypesDont()
    {
        var noNullsException = Record.Exception(() => new NullablePropertyPocoKeySetStrategy.Cursor(
            NullableIntProp: 10,
            NullableGuidProp: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            NullableBoolProp: false,
            NullableStringProp: "abc"));
        Assert.Null(noNullsException);

        // Null int?
        Assert.Throws<KeySetCursorNullValueException>(() => new NullablePropertyPocoKeySetStrategy.Cursor(
            NullableIntProp: null,
            NullableGuidProp: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            NullableBoolProp: false,
            NullableStringProp: "abc"));

        // Null Guid?
        Assert.Throws<KeySetCursorNullValueException>(() => new NullablePropertyPocoKeySetStrategy.Cursor(
            NullableIntProp: 10,
            NullableGuidProp: null,
            NullableBoolProp: false,
            NullableStringProp: "abc"));

        // Null bool?
        Assert.Throws<KeySetCursorNullValueException>(() => new NullablePropertyPocoKeySetStrategy.Cursor(
            NullableIntProp: 10,
            NullableGuidProp: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            NullableBoolProp: null,
            NullableStringProp: "abc"));

        // Null string? -> not a value type
        var nullStringException = Record.Exception(() => new NullablePropertyPocoKeySetStrategy.Cursor(
            NullableIntProp: 10,
            NullableGuidProp: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            NullableBoolProp: false,
            NullableStringProp: null));
        Assert.Null(nullStringException);
    }
}
