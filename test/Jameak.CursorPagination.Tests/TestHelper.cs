using System.Text.RegularExpressions;
using Jameak.CursorPagination.Abstractions;
using Jameak.CursorPagination.Abstractions.Enums;
using Jameak.CursorPagination.Tests.DbClasses;
using Jameak.CursorPagination.Tests.InputClasses;
using Microsoft.EntityFrameworkCore;

namespace Jameak.CursorPagination.Tests;
public static partial class TestHelper
{
    public static void CombinePageHelper<T, TCursor>(LinkedList<RowData<T, TCursor>> combinedList, IReadOnlyList<RowData<T, TCursor>> pageList, PaginationDirection direction) where TCursor : ICursor
    {
        if (direction == PaginationDirection.Forward)
        {
            foreach (var item in pageList)
            {
                combinedList.AddLast(item);
            }
        }
        else
        {
            // Since we're paginating backwards, add each element to the front of the list to get a consistently ordered result at the end.
            for (var i = pageList.Count - 1; i >= 0; i--)
            {
                combinedList.AddFirst(pageList[i]);
            }
        }
    }

    public static TCursor? GetExpectedNextCursor<T, TCursor>(IReadOnlyList<RowData<T, TCursor>> data, PaginationDirection direction) where TCursor : class, ICursor
    {
        return direction == PaginationDirection.Forward
            ? data.LastOrDefault()?.Cursor
            : data.FirstOrDefault()?.Cursor;
    }

    public static VerifySettings CreateVerifierSettings(params object?[] parameters)
    {
        var settings = new VerifySettings();
        if (parameters.Length != 0)
        {
            settings.UseParameters(parameters);
        }

        settings.ScrubLinesWithReplace(line => DurationLogRegex().Replace(line, "(scrubbed execution time)"));
        return settings;
    }

    public static IQueryable<T> TagTestQueryable<T>(IQueryable<T> queryable)
    {
        return queryable.TagWith("TaggedTestQueryable");
    }

    public static IEnumerable<string> TaggedLogMessages(TestDbContext dbContext)
    {
        return dbContext.LogMessages.Where(e => e.Contains("TaggedTestQueryable"));
    }

    public static List<SimplePropertyPoco> CreateSimplePropertyPocoData()
    {
        return
        [
            new()
            {
                IntProp = 1,
                StringProp = "a"
            },
            new()
            {
                IntProp = 2,
                StringProp = "b"
            },
            new()
            {
                IntProp = 3,
                StringProp = "b"
            },
            new()
            {
                IntProp = 3,
                StringProp = "c"
            },
            new()
            {
                IntProp = 3,
                StringProp = "d"
            },
            new()
            {
                IntProp = 4,
                StringProp = "e"
            },
            new()
            {
                IntProp = 5,
                StringProp = "e"
            },
            new()
            {
                IntProp = 6,
                StringProp = "e"
            }
        ];
    }

    public static List<SimpleFieldPoco> CreateSimpleFieldPocoData()
    {
        return
        [
            new()
            {
                IntField = 1,
                StringField = "a"
            },
            new()
            {
                IntField = 2,
                StringField = "b"
            },
            new()
            {
                IntField = 3,
                StringField = "b"
            },
            new()
            {
                IntField = 3,
                StringField = "c"
            },
            new()
            {
                IntField = 3,
                StringField = "d"
            },
            new()
            {
                IntField = 4,
                StringField = "e"
            },
            new()
            {
                IntField = 5,
                StringField = "e"
            },
            new()
            {
                IntField = 6,
                StringField = "e"
            }
        ];
    }

    public static List<PocoWithNestedProperty> CreatePocoWithNestedPropertyData()
    {

        return
        [
            new()
            {
                IntProp = 1,
                NestedData = new(){
                    StringProp = "a",
                    IntProp = 7
                }
            },
            new()
            {
                IntProp = 2,
                NestedData = new(){
                    StringProp = "a",
                    IntProp = 6
                }
            },
            new()
            {
                IntProp = 3,
                NestedData = new(){
                    StringProp = "a",
                    IntProp = 5
                }
            },
            new()
            {
                IntProp = 4,
                NestedData = new(){
                    StringProp = "b",
                    IntProp = 4
                }
            },
            new()
            {
                IntProp = 5,
                NestedData = new(){
                    StringProp = "c",
                    IntProp = 3
                }
            },
            new()
            {
                IntProp = 6,
                NestedData = new(){
                    StringProp = "a",
                    IntProp = 2
                }
            },
            new()
            {
                IntProp = 7,
                NestedData = new(){
                    StringProp = "d",
                    IntProp = 1
                }
            },
        ];
    }

    [GeneratedRegex(@"\(\d+ms\)")]
    private static partial Regex DurationLogRegex();
}
