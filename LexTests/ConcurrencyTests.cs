using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LexTests;

public class ConcurrencyTests
{
    // A source generator may run several .lex files at once.
    [Fact]
    public void Parallel_generation_matches_sequential()
    {
        var specs = new[] { "simple", "sample" }
            .Select(n => File.ReadAllText(Harness.LexFile(n + ".lex"))).ToArray();
        var expected = specs.Select(Harness.Generate).ToArray();
        var results = new string[64];
        Parallel.For(0, results.Length, new ParallelOptions { MaxDegreeOfParallelism = 8 },
            i => results[i] = Harness.Generate(specs[i % 2]));
        for (int i = 0; i < results.Length; i++)
            Assert.Equal(expected[i % 2], results[i]);
    }
}
