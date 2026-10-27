using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using LexLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

// Tests redirect Console, which is process-wide.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LexTests;

/// <summary>
/// Helpers shared by the tests: run the generator, compile what it emits,
/// drive the compiled lexer, and compare against golden files.
/// </summary>
public static class Harness
{
    public static string LexFile(string name) =>
        Path.Combine(AppContext.BaseDirectory, "LexFiles", name);

    /// <summary>Run the generator on spec text; returns emitted C# source.</summary>
    public static string Generate(string specText) => LexGenerator.Generate(specText);

    /// <summary>Compile generated source into an in-memory assembly.</summary>
    public static Assembly Compile(string source) =>
        Compile(source, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    /// <summary>Compile with the given options; warnings promoted to errors fail too.</summary>
    public static Assembly Compile(string source, CSharpCompilationOptions options)
    {
        var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            // The runtime base class, for lexers generated with %runtime.
            .Append(MetadataReference.CreateFromFile(typeof(CsLexLib.CsLex<>).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "GeneratedLexer_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source) },
            refs,
            options);
        var pe = new MemoryStream();
        var result = compilation.Emit(pe);
        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());
            throw new InvalidOperationException(
                "Generated lexer failed to compile:\n" + string.Join("\n", errors));
        }
        pe.Position = 0;
        return new AssemblyLoadContext(null, isCollectible: true).LoadFromStream(pe);
    }

    /// <summary>
    /// Run a compiled lexer over input text. Returns a transcript holding
    /// everything the actions printed plus each returned token, in order, in
    /// the same form as the sample specs' own Main() prints them.
    /// </summary>
    public static string Run(Assembly asm, string input,
        string className = "YyNameSpace.Yylex", string function = "yylex", int maxTokens = 100000)
    {
        var type = asm.GetType(className, throwOnError: true);
        var ctor = type.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null, new[] { typeof(TextReader) }, null);
        var lexer = ctor.Invoke(new object[] { new StringReader(input) });
        var next = type.GetMethod(function,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var transcript = new StringWriter { NewLine = "\n" };
        var saved = Console.Out;
        Console.SetOut(transcript);
        try
        {
            for (int count = 0; count < maxTokens; count++)
            {
                object token;
                try
                {
                    token = next.Invoke(lexer, null);
                }
                catch (TargetInvocationException e)
                {
                    transcript.WriteLine("!! " + e.InnerException.GetType().Name
                        + ": " + e.InnerException.Message);
                    break;
                }
                // null, or YYEOF (-1) from an %integer lexer, ends the input.
                if (token == null || (token is int eof && eof == -1))
                    break;
                transcript.WriteLine(token);
            }
        }
        finally
        {
            Console.SetOut(saved);
        }
        return transcript.ToString();
    }

    /// <summary>
    /// Compare against Golden/&lt;name&gt;. Set CSLEX_UPDATE_GOLDEN=1 to
    /// rewrite the checked-in golden file instead of failing (except
    /// token transcripts, which only tools/original-reference writes).
    /// </summary>
    public static void AssertGolden(string name, string actual,
        [CallerFilePath] string callerFile = "")
    {
        actual = actual.Replace("\r\n", "\n");
        var sourcePath = Path.Combine(Path.GetDirectoryName(callerFile), "Golden", name);
        // *.tokens.txt come from the original generator, never from the
        // code under test; see tools/original-reference.
        if (Environment.GetEnvironmentVariable("CSLEX_UPDATE_GOLDEN") == "1"
            && !name.EndsWith(".tokens.txt", StringComparison.Ordinal))
        {
            File.WriteAllText(sourcePath, actual);
            return;
        }
        var path = Path.Combine(AppContext.BaseDirectory, "Golden", name);
        Assert.True(File.Exists(path),
            $"Golden file {name} is missing; run with CSLEX_UPDATE_GOLDEN=1 to create it.");
        var expected = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.Equal(expected, actual);
    }
}
