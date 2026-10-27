using Xunit;

namespace LexTests;

/// <summary>
/// Character handling across the three alphabets: 7-bit (default), %full
/// (8-bit) and %unicode (UTF-16 code units).
/// </summary>
public class UnicodeTests
{
    // Rules: words, any other single character, newline. Each token is
    // returned as "<text>" so the transcript shows exact boundaries.
    static string Spec(string directive) =>
        "%%\n%type string\n" + directive + "\n%%\n"
        + "[a-z]+ { return \"<\" + yytext() + \">\"; }\n"
        + ". { return \"<\" + yytext() + \">\"; }\n"
        + "\\n { return \"<NL>\"; }\n";

    static string Lex(string directive, string input) =>
        Harness.Run(Harness.Compile(Harness.Generate(Spec(directive))), input);

    [Fact]
    public void Default_alphabet_rejects_non_ascii_cleanly()
    {
        var transcript = Lex("", "abéc\n");
        Assert.Equal("<ab>\n!! ApplicationException: Lexical Error: Unmatched Input.\n", transcript);
    }

    [Fact]
    public void Default_alphabet_does_not_mistake_u0081_for_end_of_input()
    {
        // 128/129 used to be the BOL/EOF pseudo-characters, so U+0081 in the
        // input silently ended the token stream.
        var transcript = Lex("", "ab\u0081cd\n");
        Assert.Equal("<ab>\n!! ApplicationException: Lexical Error: Unmatched Input.\n", transcript);
    }

    [Fact]
    public void Full_alphabet_accepts_latin1_and_rejects_above()
    {
        Assert.Equal("<caf>\n<é>\n<NL>\n", Lex("%full", "café\n"));
        Assert.Equal("<a>\n!! ApplicationException: Lexical Error: Unmatched Input.\n",
            Lex("%full", "aĀ"));
    }

    [Fact]
    public void Unicode_alphabet_accepts_any_utf16_code_unit()
    {
        Assert.Equal("<caf>\n<é>\n< >\n<€>\n<￿>\n<NL>\n",
            Lex("%unicode", "café €￿\n"));
    }

    [Fact]
    public void Unicode_alphabet_supports_non_ascii_character_classes()
    {
        var spec = "%%\n%type string\n%unicode\n%%\n"
            + "[α-ω]+ { return \"greek:\" + yytext(); }\n"
            + "[^α-ω] { return \"other:\" + yytext(); }\n";
        var transcript = Harness.Run(Harness.Compile(Harness.Generate(spec)),
            "λογος x");
        Assert.Equal("greek:λογος\nother: \nother:x\n", transcript);
    }

    [Fact]
    public void Unicode_alphabet_keeps_bol_anchor_working()
    {
        var spec = "%%\n%type string\n%unicode\n%%\n"
            + "^é { return \"start-e\"; }\n"
            + "é { return \"e\"; }\n"
            + "\\n { return \"NL\"; }\n";
        var transcript = Harness.Run(Harness.Compile(Harness.Generate(spec)),
            "éé\né\n");
        Assert.Equal("start-e\ne\nNL\nstart-e\nNL\n", transcript);
    }

    [Fact]
    public void Unicode_escapes_in_patterns()
    {
        var spec = "%%\n%type string\n%unicode\n%%\n"
            + "[\\u03b1-\\u03c9]+ { return \"greek:\" + yytext(); }\n"
            + "\\u20ac { return \"euro\"; }\n";
        var transcript = Harness.Run(Harness.Compile(Harness.Generate(spec)),
            "\u03b1\u03b2\u20ac");
        // Inside "..." only \" is an escape (see CsLex.md), so \u is not
        // tested there.
        Assert.Equal("greek:\u03b1\u03b2\neuro\n", transcript);
    }

    [Fact]
    public void Ignorecase_folds_non_ascii_letters()
    {
        var spec = "%%\n%type string\n%unicode\n%ignorecase\n%%\n"
            + "\\u00e9+ { return \"e:\" + yytext(); }\n"
            + "[\\u03b1-\\u03c9]+ { return \"greek:\" + yytext(); }\n";
        var transcript = Harness.Run(Harness.Compile(Harness.Generate(spec)),
            "\u00c9\u00e9\u00c9\u0391\u03b2");
        Assert.Equal("e:\u00c9\u00e9\u00c9\ngreek:\u0391\u03b2\n", transcript);
    }
}
