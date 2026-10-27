using System.IO;
using Xunit;

namespace LexTests
{
    /// <summary>
    /// Characterization tests: pin the generator's output and the behavior of
    /// the lexers it generates, so refactoring can be checked against them.
    /// </summary>
    public class GoldenTests
    {
        [Theory]
        [InlineData("simple")]
        [InlineData("sample")]
        public void Generate_matches_golden_source(string spec)
        {
            var source = Harness.Generate(File.ReadAllText(Harness.LexFile(spec + ".lex")));
            Harness.AssertGolden(spec + ".generated.cs.txt", source);
        }

        // Golden tokens are the output of lexers built by the original 1999
        // generator (lex/src) on .NET 10, so this pins the original behavior.
        [Theory(Skip = "Bug: emitted lexer does not compile; see KnownBugTests.")]
        [InlineData("simple")]
        [InlineData("sample")]
        public void Generated_lexer_matches_golden_tokens(string spec)
        {
            var source = Harness.Generate(File.ReadAllText(Harness.LexFile(spec + ".lex")));
            var asm = Harness.Compile(source);
            var transcript = Harness.Run(asm, File.ReadAllText(Harness.LexFile("test.txt")));
            Harness.AssertGolden(spec + ".tokens.txt", transcript);
        }
    }
}
