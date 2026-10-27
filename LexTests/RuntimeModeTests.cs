using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace LexTests;

/// <summary>
/// %runtime: the generated lexer derives from CsLexLib.CsLex&lt;T&gt;
/// instead of carrying its own buffer and driver. It must behave exactly
/// like the standalone form.
/// </summary>
public class RuntimeModeTests
{
    // Put %runtime first in the declarations section.
    static string WithRuntime(string spec)
    {
        int i = spec.IndexOf("%%\n") + 3;
        return spec.Substring(0, i) + "%runtime\n" + spec.Substring(i);
    }

    // %eofval keeps returning its value at end of input, so cap the tokens.
    static string Lex(string spec, string input, string cls = "YyNameSpace.Yylex", string fn = "yylex") =>
        Harness.Run(Harness.Compile(Harness.Generate(spec)), input, cls, fn, maxTokens: 20);

    [Theory]
    [InlineData("simple")]
    [InlineData("sample")]
    public void Sample_specs_match_the_original_generator(string spec)
    {
        var text = WithRuntime(File.ReadAllText(Harness.LexFile(spec + ".lex")));
        Harness.AssertGolden(spec + ".tokens.txt",
            Lex(text, File.ReadAllText(Harness.LexFile("test.txt"))));
    }

    [Fact]
    public void Generated_source_matches_golden()
    {
        var text = WithRuntime(File.ReadAllText(Harness.LexFile("simple.lex")));
        Harness.AssertGolden("simple.runtime.generated.cs.txt", Harness.Generate(text));
    }

    [Fact]
    public void Generated_source_compiles_cleanly_in_a_strict_nullable_project()
    {
        var text = WithRuntime(File.ReadAllText(Harness.LexFile("sample.lex")));
        Harness.Compile(Harness.Generate(text), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable,
            generalDiagnosticOption: ReportDiagnostic.Error));
    }

    [Theory]
    // states, yybegin, yytext, %line/%char, %init, %eof, %eofval, renamed class/function
    [InlineData("public interface IMarker {}\n%%\n%type string\n%class Scanner\n%function Next\n%implements IMarker\n%public\n%line\n%char\n"
        + "%state QUOTE\n%{\nint count;\n%}\n%init{\ncount = 100;\n%init}\n%eof{\nSystem.Console.WriteLine(\"eof code\");\n%eof}\n"
        + "%eofval{\nreturn \"<eof \" + count + \">\";\n%eofval}\n%%\n"
        + "<YYINITIAL> [a-z]+ { count++; return yytext() + \"@\" + yyline + \":\" + yychar; }\n"
        + "<YYINITIAL> \\\" { yybegin(QUOTE); return \"open\"; }\n"
        + "<QUOTE> [^\\\"]+ { return \"text(\" + yytext() + \",\" + yylength() + \")\"; }\n"
        + "<QUOTE> \\\" { yybegin(YYINITIAL); return \"close\"; }\n"
        + "[ \\n] { return null; }\n",
        "ab cd\n\"x y\" ef")]
    // %integer
    [InlineData("%%\n%integer\n%%\n[0-9]+ { return int.Parse(yytext()); }\n\" \" { return 0; }\n", "12 345")]
    // %intwrap: boxed int (object), where null means keep scanning
    [InlineData("%%\n%intwrap\n%%\n[0-9]+ { return int.Parse(yytext()); }\n\" \" { return null; }\n", "12 345")]
    // %unicode, and input outside a 7-bit alphabet
    [InlineData("%%\n%type string\n%unicode\n%%\n[\\u03b1-\\u03c9]+ { return \"g:\" + yytext(); }\n. { return yytext(); }\n",
        "\u03b1\u03b2-\u20ac")]
    [InlineData("%%\n%type string\n%%\n[a-z]+ { return yytext(); }\n", "ab\u00e9")]
    public void Runtime_and_standalone_lexers_behave_the_same(string spec, string input)
    {
        bool renamed = spec.Contains("%class Scanner");
        string cls = renamed ? "YyNameSpace.Scanner" : "YyNameSpace.Yylex";
        string fn = renamed ? "Next" : "yylex";
        var standalone = Lex(spec, input, cls, fn);
        var runtime = Lex(WithRuntime(spec), input, cls, fn);
        Assert.Equal(standalone, runtime);
    }
}
