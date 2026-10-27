#nullable enable

using System;
using System.IO;
using LexLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CsLex.SourceGenerator;

/// <summary>
/// Generates a lexer for each <c>.lex</c> file in AdditionalFiles. The
/// output is named <c>&lt;file&gt;.lex.g.cs</c>, so two .lex files in one
/// project need different file names.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class LexSourceGenerator : IIncrementalGenerator
{
    internal static readonly DiagnosticDescriptor SpecError = new DiagnosticDescriptor(
        id: "CSLEX001",
        title: "Invalid lex specification",
        messageFormat: "{0}",
        category: "CsLex",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var specs = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".lex", StringComparison.OrdinalIgnoreCase))
            .Select((file, cancel) => (file.Path, Text: file.GetText(cancel)?.ToString()));

        context.RegisterSourceOutput(specs, (output, spec) =>
        {
            if (spec.Text == null)
                return;
            try
            {
                var source = LexGenerator.Generate(spec.Text);
                output.AddSource(Path.GetFileNameWithoutExtension(spec.Path) + ".lex.g.cs", source);
            }
            catch (LexSpecException e)
            {
                var line = Math.Max(0, e.Line - 1);
                var span = new LinePositionSpan(new LinePosition(line, 0), new LinePosition(line, 0));
                output.ReportDiagnostic(Diagnostic.Create(SpecError,
                    Location.Create(spec.Path, default, span), e.Message.Replace("\n", " ")));
            }
        });
    }
}
