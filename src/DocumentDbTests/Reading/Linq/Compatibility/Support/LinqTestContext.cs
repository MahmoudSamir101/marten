using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;
using Marten;
using Marten.Testing.Documents;
using Xunit;

namespace DocumentDbTests.Reading.Linq.Compatibility.Support;

[CollectionDefinition("linq")]
public class LinqCollection: ICollectionFixture<DefaultQueryFixture>
{

}

[Collection("linq")]
public abstract class LinqTestContext<TSelf>
{
    protected static IList<LinqTestCase> testCases = new List<LinqTestCase>();

    public DefaultQueryFixture Fixture { get; }

    static LinqTestContext()
    {
        _descriptions = readDescriptions();
    }

    protected LinqTestContext(DefaultQueryFixture fixture)
    {
        Fixture = fixture;
    }

    protected static void @where(Expression<Func<Target, bool>> expression)
    {
        var comparison = new TargetComparison(q => q.Where(expression)) { Ordered = false };

        testCases.Add(comparison);
    }

    protected static void ordered(Func<IQueryable<Target>, IQueryable<Target>> func)
    {
        var comparison = new TargetComparison(func) { Ordered = true };

        testCases.Add(comparison);
    }

    protected static void unordered(Func<IQueryable<Target>, IQueryable<Target>> func)
    {
        var comparison = new TargetComparison(func) { Ordered = false };

        testCases.Add(comparison);
    }

    protected static void selectInOrder<T>(Func<IQueryable<Target>, IQueryable<T>> selector)
    {
        var comparison = new OrderedSelectComparison<T>(selector);
        testCases.Add(comparison);
    }

    private static string[] _methodNames = new string[] { "@where", nameof(ordered), nameof(unordered), nameof(selectInOrder) };
    private static string[] _descriptions;

    protected static string[] readDescriptions()
    {
        var path = AppContext.BaseDirectory;
        while (!path.EndsWith("DocumentDbTests"))
        {
            path = path.ParentDirectory();
        }

        var filename = typeof(TSelf).Name + ".cs";
        var codefile = path.AppendPath("Reading", "Linq", "Compatibility", filename);

        var list = new List<string>();

        new FileSystem().ReadTextFile(codefile, line =>
        {
            line = line.Trim();

            if (_methodNames.Any(x => line.StartsWith(x)))
            {
                var start = line.IndexOf('(') + 1;

                var description = line.Substring(start, line.Length - start - 2);

                list.Add(description);
            }
        });

        return list.ToArray();
    }

    public static IEnumerable<object[]> GetDescriptions()
    {
        return _descriptions.Select(x => new object[] { x });
    }

    protected async Task assertTestCase(string description, IDocumentStore store)
    {
        var index = _descriptions.IndexOf(description);

        var testCase = testCases[index];
        using (var session = store.QuerySession())
        {
            await testCase.Compare(session, Fixture.Documents);
        }
    }
}