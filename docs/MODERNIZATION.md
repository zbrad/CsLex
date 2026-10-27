# CsLex Modernization: Review, Recommendations, and Plan

Reviewed 2026-10-27 at commit `a50a7dd`. Nothing was compiled or run during this
review: the host had no .NET SDK. Every finding below comes from reading source.
Where a finding has not yet been confirmed by a test, it says so.

## 1. What is in the repo today

| Path | What it is |
|---|---|
| `lex/src/` | The original 1999 port of JLex. Non-generic `Hashtable`/`ArrayList` code, built with `nmake`. Not in `CsLex.sln`. |
| `LexLib/` | The generator as a class library (2015). It is `lex/src` with generics added. Ignoring whitespace and line endings, most files differ only slightly; `Emit.cs` (template/format-string rewrite), `Gen.cs` and `Spec.cs` changed the most. |
| `CsLexConsole/` | CLI driver for `LexLib`. |
| `CsLexLib/` | Experimental runtime base class `CsLex<T>`, holding the buffer/advance/driver logic so generated code could be smaller (the last commit). |
| `CsLexLibTest/` | Hand-written lexer on top of `CsLexLib`. |
| `LexTests/` | MSTest project. It has one test, and that test asserts nothing. |
| `TestOutputSimple/`, `LexTests/TestOutput*.cs` | Generator output saved as source and reformatted by the IDE, so not byte-exact baselines. |
| `CsLex.md` | Manual, with the CsLex (2000) and JLex copyright notices. |

Every project is an old-style (non-SDK) `.csproj` targeting .NET Framework 4.5.2,
with `ToolsVersion=14.0` and a VS2015 `.sln`. They will not build with
`dotnet build` on Linux or macOS as they are.

## 2. Bugs to fix regardless of modernization

Ordered by impact. Each one gets a reproducing test before its fix.

1. **The CLI ignores its argument.** [CsLexConsole/Program.cs](../CsLexConsole/Program.cs)
   checks `args.Length < 2` and reads `args[1]`. Those indices were carried over
   from the original `Main()`, which read `Environment.GetCommandLineArgs()`
   (index 0 there is the exe). With `Main(string[] args)`, `lex foo.lex` prints
   usage and exits. *Confirmed by reading both files.* The `instream == null`
   check after `new StreamReader(...)` can never be true.
2. **The library kills its host process.** `Error.parse_error` calls
   `Environment.Exit(1)` ([LexLib/Error.cs](../LexLib/Error.cs)). A spec error
   inside the test runner, an IDE, or a source generator ends that process.
   `Error` is also in the global namespace, and the library makes about 190
   `Console.Write*` calls (progress dots, "Working on character classes.", and
   others).
3. **Characters outside the configured range crash the generated lexer**, and
   two real characters are misread as control signals:
   - The lookahead is a `char`, and BOL/EOF are pseudo-characters numbered just
     past the alphabet: 128/129 by default, 257/258 with `%full`
     ([LexLib/MakeNfa.cs](../LexLib/MakeNfa.cs) `Allocate_BOL_EOF`). An input
     U+0080 is read as BOL and U+0081 as EOF.
   - `yy_cmap` has only `dtrans_ncols` entries, so `yy_cmap[yy_lookahead]`
     throws `IndexOutOfRangeException` for any larger character, such as `é` or
     `€`.
   - `%unicode` is parsed and then ignored (`/* UNDONE: What to do here? */` in
     [LexLib/Gen.cs](../LexLib/Gen.cs)).
   - The `CsLexLib` base class hard-codes `YY_BOL = 128` and `YY_EOF = 129`,
     while the generator emits `spec.BOL`/`spec.EOF`. The base class is
     therefore only correct for 7-bit specs.
4. **Suspected: `BitSet.binop` reads stale blocks.** It loops to `bits.Length`
   (the array capacity) instead of `inuse`, and `ClearAll()` only resets
   `inuse`. `SimplifyNFA` calls `ClearAll()` and then `And()` on every NFA
   state, so stale blocks with unsorted offsets can be merged into the result.
   That would corrupt character-class splitting. *Not yet confirmed; write a
   failing unit test first.* Related:
   - In the empty-result branch, `binop` assigns to `this.bits` rather than
     `a.bits`. That's harmless today, because every caller passes `this` as `a`.
   - `GetHashCode` computes `bits[i] * offs[i]`, so block 0 (bits 0–63) never
     affects the hash. In a spec with fewer than 64 NFA states, every
     `dfa_sets` key hashes the same, and the dictionary degrades to a linear
     scan. This is a performance problem, not a correctness one. `CompareTo` orders by `inuse` before
   content, which disagrees with `Equals`. That doesn't matter while BitSet is
   only a dictionary key, but it is wrong.
5. **Stray debug output:** `CharSet.add` prints `"i = 0"`.
6. **`Utility.assert` calls are wrapped in `#if DEBUG`** (the method itself
   isn't), so Debug and Release builds behave differently on invalid input.

## 3. Modernization recommendations

### Build and project layout
- Convert every project to SDK-style `.csproj` files and replace the solution
  with `CsLex.slnx`, or keep a regenerated `.sln`. Add `Directory.Build.props`
  with `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors` (enabled
  per project once that project is clean), and deterministic builds.
- **Target frameworks.** .NET 8 and .NET 9 reach end of support on
  2026-11-10. .NET 10 is the current LTS, supported until November 2028.[^net]
  - Generator library and CLI: `net10.0`.
  - Runtime support library (if kept): `netstandard2.0`, so generated lexers
    work everywhere.
  - Roslyn source generator, if built: `netstandard2.0`, which Roslyn
    requires. (This last point is from my memory of the Roslyn rules, not
    checked in this review.)
- Remove `App.config`, `Properties/AssemblyInfo.cs`, and `Settings.settings`.
  Two settings are read: `MaxBuf` (CLI) and `MaxStr` (five places in `Emit`,
  `Gen`, and `Nfa2Dfa`). Both become constants or options.
- Add a GitHub Actions workflow that builds and tests on Linux and Windows, with
  actions pinned to commit SHAs.
- Remove `lex/src` from the main branch after tagging it (for example
  `original-1999`), so there is one copy of the generator. Git history keeps the
  original.

### Library API (LexLib)
- Report errors as data: a `LexDiagnostic(code, message, line, column)` list,
  plus a `LexSpecException` for fatal errors. No `Console`, no `Exit`.
- Add a small public entry point, for example
  `LexGenerator.Generate(TextReader spec, TextWriter output, LexOptions options)`
  returning a result with diagnostics. The large set of `public` fields on
  `Spec`, `Gen`, and the other classes becomes `internal`.
- Use one namespace (`CsLex` or `CsLex.Generator`) and file-scoped namespaces.
  Fix the leftover global `Error` class.
- Replace the hand-rolled `Utility` helpers (`IsSpace`, `Compare`,
  `INT_MAX`, the debug `enter`/`leave`) with BCL calls or `Debug.Assert`.
- `BitSet`: either fix it and cover it with tests, or replace it with a sorted
  `ulong[]` plus `System.Numerics.BitOperations` for enumeration. Check that
  generation time doesn't regress on a large spec.
- C# idioms, in a pass of their own after the tests exist: `var`, expression
  bodies, `switch` expressions (`Error.errmsg` is an obvious one),
  `StringBuilder` and interpolated raw strings in `Emit`, `foreach` instead of
  index loops where the index isn't needed, and `readonly` fields.

### Generated code
- Emit modern code: file-scoped namespace, `#nullable enable`, `sealed`
  classes, `ReadOnlySpan<char>` for the current token text (keep `yytext()`
  returning `string`), and a `<auto-generated/>` header so analyzers skip the
  file.
- Make the lookahead an `int`. Put BOL and EOF above the character range (or
  use negative sentinels), and look characters up through a range table: a
  sorted array of range starts plus a binary search, or a two-level 256×256
  page table. That handles all 65,536 UTF-16 code units without emitting a
  65,536-entry `yy_cmap`. This is the change that makes `%unicode` real.
- Choose between the two runtime models:
  - **(a)** standalone generated lexer, as today;
  - **(b)** a `CsLex.Runtime` package with the `CsLex<T>` base class, which is
    the 2015 experiment;
  - **(c)** both, through a directive.

  Recommendation: keep (a) as the default, since it has no runtime dependency,
  and treat (b) as optional after the Unicode work. Sharing one emitter for the
  table layout keeps them from drifting apart.

### Distribution
- Ship the CLI as a `dotnet tool` (`dotnet tool install -g cslex`).
- Add an MSBuild integration or an incremental Roslyn source generator that
  takes `*.lex` files from `AdditionalFiles`. This is the biggest usability
  gain: no checked-in generated code and no manual generation step. Item 2 of
  section 2 has to be fixed first.
- Put package metadata (license expression, README, SourceLink) in
  `Directory.Build.props`.

### Licensing housekeeping
`LICENSE` is MIT, copyright 2015 Brad Merrill, and does not mention JLex. The
JLex notice (copyright 1996–2000 Elliot Joel Berk and C. Scott Ananian) appears
only at the end of `CsLex.md`, and it asks that the notice "appear in all
copies". Before publishing a package, add a `NOTICE` or `THIRD-PARTY-NOTICES`
file with the JLex text and include it in the packages. (This reads the notice
text as written; it is not legal advice.)

### Docs
- The `README` building section still describes `nmake`. Update it when phase 1
  lands.
- `CsLex.md` says input is "Ascii text files, with byte sized characters".
  Update that section when Unicode support lands.

## 4. Phased plan

Each phase is a sequence of small commits, one logical slice per commit.

**Phase 0: build and characterization tests (do first; everything else depends
on it)**
1. Convert the projects to SDK style and get `dotnet build` working on
   `net10.0`. The only code changes are swapping `Properties.Settings.Default.MaxStr`
   and `MaxBuf` for constants, since `ApplicationSettingsBase` isn't in .NET
   without an extra package. Leave `LexTests` out until step 2: it references
   MSTest v1 assemblies that ship with Visual Studio. → verify: `dotnet build`
   passes on Linux.
2. Replace `LexTests` with xUnit or MSTest v3 and add golden tests: generate
   from `simple.lex` and `sample.lex` and compare to checked-in `.verified.cs`
   files (Verify or a plain file compare). → verify: the tests pass against
   today's generator.
3. Add end-to-end tests: compile the generated lexer in the test (Roslyn
   `CSharpCompilation`) and run it on `test.txt`. Compare the token stream to a
   golden file. → verify: they pass. This is the safety net for every later
   change to the emitter.
4. Add failing tests for the bugs in section 2: the CLI argument, a non-ASCII
   input character, BitSet after `ClearAll`, and a parse error that must not
   exit. Mark them skipped until they are fixed.

**Phase 1: correctness fixes.** One commit per bug in section 2, each turning
its skipped test on.

**Phase 2: library hygiene.**
- Diagnostics in place of `Console` and `Exit`.
- Public entry point; `internal` everything else.
- Namespaces, file-scoped namespaces.
- Nullable annotations, one project at a time.
- Remove `lex/src` (after tagging it), the stray `LexLib/LexLib.sln`, and the
  obsolete `TestOutput*` projects, which the phase 0 golden files replace.

**Phase 3: Unicode.** Add the `int` lookahead, out-of-range BOL/EOF, and the
range-based character map, and make `%unicode` the 16-bit alphabet. Golden
output changes in this phase are expected and get reviewed by hand. Decide
whether surrogate pairs (code points above U+FFFF) are in scope; see section 5.

**Phase 4: generated-code modernization and the runtime library.** Bring the
emitted code up to date. Choose runtime model (a), (b), or (c), and fix the
base class's hard-coded BOL/EOF if it is kept.

**Phase 5: distribution.**
- Ship the `dotnet tool`.
- Build the source generator or MSBuild task.
- Add the NuGet packages and `NOTICE`.
- Add CI.
- Update the README and manual.

## 5. Decisions for you (with recommended defaults)

| Decision | Recommended default |
|---|---|
| Keep `lex/src`? | Tag it as `original-1999` and remove it from master. |
| Runtime model | Standalone generated code by default; base class later and optional. |
| Unicode scope | All UTF-16 code units in phase 3. Full code points (surrogate pairs in character classes) as a later, separate feature. |
| Legacy names (`Yylex`, `yylex`, `yytext`, `%eofval{`) | Keep them for spec compatibility with existing `.lex` files. Add modern aliases only if needed. |
| Test framework | xUnit with Verify for the golden files. |

[^net]: Microsoft, ".NET 8 and .NET 9 will reach End of Support on November 10, 2026",
https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/; .NET support
policy, https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
(accessed 2026-10-27).
