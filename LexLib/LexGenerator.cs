using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LexTests")]

#nullable enable

namespace LexLib;

/// <summary>
/// Public entry point: turns a lex specification into C# lexer source.
/// </summary>
#if CSLEX_EMBEDDED
internal
#else
public
#endif
static class LexGenerator
{
    /// <summary>
    /// Read a specification from <paramref name="spec"/> and write the
    /// generated lexer to <paramref name="output"/>.
    /// </summary>
    /// <param name="log">Receives progress messages; null discards them.</param>
    /// <exception cref="LexSpecException">The specification is invalid.</exception>
    public static void Generate(TextReader spec, TextWriter output, TextWriter? log = null)
    {
        new Gen(spec, output, log).generate();
        output.Flush();
    }

    /// <summary>
    /// Like Generate, but reports errors instead of throwing: the result holds
    /// the source, or every error found (the parser recovers at each
    /// declaration line and each rule, so one bad rule doesn't hide the next).
    /// </summary>
    public static LexResult TryGenerate(TextReader spec, TextWriter? log = null)
    {
        var output = new StringWriter();
        try
        {
            new Gen(spec, output, log).generate();
        }
        catch (LexSpecException e)
        {
            return new LexResult(null, e.Diagnostics);
        }
        return new LexResult(output.ToString(), new LexDiagnostic[0]);
    }

    /// <summary>TryGenerate from specification text.</summary>
    public static LexResult TryGenerate(string specText, TextWriter? log = null) =>
        TryGenerate(new StringReader(specText), log);

    /// <summary>Generate from specification text; returns the lexer source.</summary>
    /// <exception cref="LexSpecException">The specification is invalid.</exception>
    public static string Generate(string specText, TextWriter? log = null)
    {
        var output = new StringWriter();
        Generate(new StringReader(specText), output, log);
        return output.ToString();
    }
}
