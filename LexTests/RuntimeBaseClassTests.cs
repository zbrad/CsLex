using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace LexTests;

/// <summary>
/// The CsLexLib base class (CsLex&lt;T&gt;) driven by CsLexLibTest, a lexer
/// for simple.lex written by hand against it. The generator does not emit
/// this form; these tests keep the runtime consistent with generated lexers.
/// </summary>
public class RuntimeBaseClassTests
{
    static string RunPrototype(string inputPath)
    {
        var entry = Assembly.Load("CsLexLibTest").EntryPoint;
        var transcript = new StringWriter { NewLine = "\n" };
        var saved = Console.Out;
        Console.SetOut(transcript);
        try
        {
            entry.Invoke(null, new object[] { new[] { inputPath } });
        }
        catch (TargetInvocationException e)
        {
            transcript.WriteLine("!! " + e.InnerException.GetType().Name + ": " + e.InnerException.Message);
        }
        finally
        {
            Console.SetOut(saved);
        }
        return transcript.ToString();
    }

    [Fact]
    public void Prototype_matches_original_generator_tokens()
    {
        Harness.AssertGolden("simple.tokens.txt", RunPrototype(Harness.LexFile("test.txt")));
    }

    [Fact]
    public void Prototype_rejects_input_outside_alphabet()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "é\u0081");
        var transcript = RunPrototype(path);
        File.Delete(path);
        Assert.Equal("!! ApplicationException: Lexical Error: Unmatched Input.\n", transcript);
    }
}
