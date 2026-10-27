using System;
using System.IO;
using System.Reflection;
using LexLib;
using Xunit;

namespace LexTests
{
    /// <summary>
    /// Reproductions of the bugs listed in docs/MODERNIZATION.md section 2.
    /// Each is skipped until the commit that fixes it turns it on.
    /// </summary>
    public class KnownBugTests
    {
        const string Simple = "%%\n%%\n[a-z]+ { return yytext(); }\n. { return yytext(); }\n\\n { return yytext(); }\n";

        [Fact]
        public void Generated_lexer_compiles()
        {
            Harness.Compile(Harness.Generate(File.ReadAllText(Harness.LexFile("simple.lex"))));
        }

        [Fact]
        public void Cli_generates_output_for_single_argument()
        {
            var dir = Directory.CreateTempSubdirectory("cslex-cli-").FullName;
            var spec = Path.Combine(dir, "simple.lex");
            File.Copy(Harness.LexFile("simple.lex"), spec);
            var saved = Environment.CurrentDirectory;
            var savedOut = Console.Out;
            try
            {
                Environment.CurrentDirectory = dir;
                Console.SetOut(TextWriter.Null);
                var entry = Assembly.Load("cslex").EntryPoint;
                var exit = (int)entry.Invoke(null, new object[] { new[] { spec } });
                Assert.Equal(0, exit);
            }
            finally
            {
                Console.SetOut(savedOut);
                Environment.CurrentDirectory = saved;
            }
            var generated = File.Exists(Path.Combine(dir, "simple.cs"));
            Directory.Delete(dir, recursive: true);
            Assert.True(generated);
        }

        [Fact(Skip = "Bug: characters above the 7-bit alphabet index past yy_cmap.")]
        public void Generated_lexer_accepts_non_ascii_input()
        {
            var asm = Harness.Compile(Harness.Generate(Simple));
            var transcript = Harness.Run(asm, "café\n");
            Assert.DoesNotContain("!!", transcript);
            Assert.Contains("é", transcript);
        }

        [Fact]
        public void BitSet_hash_depends_on_low_bits()
        {
            var a = new BitSet();
            a.Set(1, true);
            var b = new BitSet();
            b.Set(2, true);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void BitSet_equal_sets_hash_equal_despite_zero_blocks()
        {
            var a = new BitSet();
            a.Set(3, true);
            a.Set(200, true);
            a.Set(200, false);   // leaves an all-zero block behind
            var b = new BitSet();
            b.Set(3, true);
            Assert.True(a.Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void BitSet_and_after_clear_ignores_stale_blocks()
        {
            var a = new BitSet();
            a.Set(5 * 64, true);
            a.Set(9 * 64, true);
            a.ClearAll();
            a.Set(0, true);

            var b = new BitSet();
            b.Set(9 * 64, true);
            a.And(b);

            Assert.False(a.Get(9 * 64), "stale block from before ClearAll() leaked into And()");
        }

        [Fact]
        public void Parse_error_reports_instead_of_exiting()
        {
            var e = Assert.Throws<LexSpecException>(
                () => Harness.Generate("%%\n%%\n<NOSUCHSTATE> a { return null; }\n"));
            Assert.Equal(Error.E_STATE, e.ErrorCode);
            Assert.Equal(3, e.Line);
            Assert.Contains("[NOSUCHSTATE]", e.Message);
        }

        [Fact]
        public void Undefined_macro_reports_instead_of_exiting()
        {
            var e = Assert.Throws<LexSpecException>(
                () => Harness.Generate("%%\n%%\n{NOSUCHMACRO} { return null; }\n"));
            Assert.Equal(Error.E_NOMAC, e.ErrorCode);
            Assert.Contains("[NOSUCHMACRO]", e.Message);
        }

        [Fact]
        public void Rule_without_state_prefix_is_accepted()
        {
            var asm = Harness.Compile(Harness.Generate(
                "%%\n%type string\n%%\n[a-z]+ { return yytext(); }\n[^a-z] { return \"<\" + yytext() + \">\"; }\n"));
            Assert.Equal("abc\n< >\nde\n", Harness.Run(asm, "abc de"));
        }
    }
}
