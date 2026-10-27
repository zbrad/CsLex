using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LexTests;

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
    [Theory]
    [InlineData("simple")]
    [InlineData("sample")]
    public void Generated_lexer_matches_golden_tokens(string spec)
    {
        var source = Harness.Generate(File.ReadAllText(Harness.LexFile(spec + ".lex")));
        var asm = Harness.Compile(source);
        var transcript = Harness.Run(asm, File.ReadAllText(Harness.LexFile("test.txt")));
        Harness.AssertGolden(spec + ".tokens.txt", transcript);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("sample")]
    public void Generated_lexer_compiles_cleanly_in_a_strict_nullable_project(string spec)
    {
        var source = Harness.Generate(File.ReadAllText(Harness.LexFile(spec + ".lex")));
        var strict = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable,
            generalDiagnosticOption: ReportDiagnostic.Error);
        Harness.Compile(source, strict);
    }
}
