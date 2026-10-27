using System.Linq;
using LexLib;
using Xunit;

namespace LexTests;

/// <summary>
/// Error recovery: a spec with several errors reports all of them, one per
/// bad declaration line or bad rule, without follow-on errors.
/// </summary>
public class DiagnosticsTests
{
    static (int Code, int Line)[] Errors(string spec)
    {
        var result = LexGenerator.TryGenerate(spec);
        Assert.False(result.Success);
        Assert.Null(result.Source);
        return result.Diagnostics.Select(d => (d.ErrorCode, d.Line)).ToArray();
    }

    [Fact]
    public void Valid_spec_succeeds_with_no_diagnostics()
    {
        var spec = "%%\n%type string\n%%\na { return \"a\"; }\n";
        var result = LexGenerator.TryGenerate(spec);
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(LexGenerator.Generate(spec), result.Source);
    }

    [Fact]
    public void Each_bad_declaration_line_is_reported()
    {
        var spec = "%%\n%bogus\n%state A\n%nosuch\n%%\n<A> a { return null; }\n";
        Assert.Equal(new[] { (Error.E_DIRECT, 2), (Error.E_DIRECT, 4) }, Errors(spec));
    }

    [Fact]
    public void Each_bad_rule_is_reported_and_multiline_actions_are_skipped()
    {
        var spec = "%%\n%type string\n%%\n"
            + "<NOSUCH> a { return \"a\"; }\n"      // line 4
            + "b {\n  return \"b\";\n}\n"
            + "{UNDEFINED} {\n  return \"c\";\n}\n"  // line 8
            + "d { return \"d\"; }\n";
        Assert.Equal(new[] { (Error.E_STATE, 4), (Error.E_NOMAC, 8) }, Errors(spec));
    }

    [Fact]
    public void Declaration_and_rule_errors_are_both_reported()
    {
        var spec = "%%\n%bogus\n%%\n<NOSUCH> a { return null; }\nb { return null; }\n";
        Assert.Equal(new[] { (Error.E_DIRECT, 2), (Error.E_STATE, 4) }, Errors(spec));
    }

    [Fact]
    public void Unterminated_action_stops_with_one_error()
    {
        var spec = "%%\n%%\na { return null;\n";
        Assert.Equal(Error.E_EOF, Errors(spec).Single().Code);
    }

    [Fact]
    public void Generate_throws_with_all_diagnostics_attached()
    {
        var spec = "%%\n%bogus\n%nosuch\n%%\na { return null; }\n";
        var e = Assert.Throws<LexSpecException>(() => LexGenerator.Generate(spec));
        Assert.Equal(Error.E_DIRECT, e.ErrorCode);
        Assert.Equal(2, e.Line);
        Assert.Equal(new[] { 2, 3 }, e.Diagnostics.Select(d => d.Line).ToArray());
    }
}
