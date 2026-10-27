extern alias sourcegen;

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using LexSourceGenerator = sourcegen::CsLex.SourceGenerator.LexSourceGenerator;

namespace LexTests;

public class SourceGeneratorTests
{
    sealed class InMemoryText : AdditionalText
    {
        readonly string text;
        public InMemoryText(string path, string text) { Path = path; this.text = text; }
        public override string Path { get; }
        public override SourceText GetText(CancellationToken cancel = default) => SourceText.From(text);
    }

    static (GeneratorDriverRunResult Result, Compilation Output) Run(params AdditionalText[] files)
    {
        var compilation = CSharpCompilation.Create("Consumer",
            references: ((string)System.AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
                .Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new LexSourceGenerator().AsSourceGenerator() },
            additionalTexts: ImmutableArray.Create(files));
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
        return (driver.GetRunResult(), output);
    }

    [Fact]
    public void Generates_same_source_as_the_library()
    {
        var spec = File.ReadAllText(Harness.LexFile("simple.lex"));
        var (result, output) = Run(new InMemoryText("/src/simple.lex", spec), new InMemoryText("/src/notes.txt", "x"));

        var generated = Assert.Single(result.GeneratedTrees);
        Assert.EndsWith("simple.lex.g.cs", generated.FilePath);
        Assert.Equal(Harness.Generate(spec).Replace("\r\n", "\n"),
            generated.GetText().ToString().Replace("\r\n", "\n"));
        Assert.Empty(result.Diagnostics);
        Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Invalid_spec_reports_diagnostic_at_its_line()
    {
        var (result, _) = Run(new InMemoryText("/src/bad.lex", "%%\n%%\n<NOSUCHSTATE> a { return null; }\n"));

        Assert.Empty(result.GeneratedTrees);
        var d = Assert.Single(result.Diagnostics);
        Assert.Equal("CSLEX001", d.Id);
        Assert.Equal(DiagnosticSeverity.Error, d.Severity);
        Assert.Equal(2, d.Location.GetLineSpan().StartLinePosition.Line);   // 0-based: line 3
        Assert.Contains("NOSUCHSTATE", d.GetMessage());
    }

    [Fact]
    public void Every_spec_error_is_reported()
    {
        var (result, _) = Run(new InMemoryText("/src/bad.lex",
            "%%\n%bogus\n%%\n<NOSUCHSTATE> a { return null; }\nb { return null; }\n"));

        Assert.Empty(result.GeneratedTrees);
        Assert.Equal(new[] { 1, 3 },   // 0-based lines 2 and 4
            result.Diagnostics.Select(d => d.Location.GetLineSpan().StartLinePosition.Line).ToArray());
        Assert.All(result.Diagnostics, d => Assert.Equal("CSLEX001", d.Id));
    }
}
