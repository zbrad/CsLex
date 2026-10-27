# CsLex

CsLex is a lexical analyzer generator for C#. It reads a lex specification
(regular expressions paired with C# actions) and writes a C# lexer class. It is
a 1999 C# port of [JLex](https://www.cs.princeton.edu/~appel/modern/java/JLex/),
updated for current .NET. The specification language is documented in the
[CsLex manual](CsLex.md).

## Use it

There are three ways to run the generator, all from the same code:

- **`dotnet tool`.** Writes `<name>.cs` into the current directory:

      dotnet tool install -g CsLex.Tool --prerelease
      cslex mylexer.lex

- **Source generator.** The lexer is generated at build time from each `.lex`
  file listed in `AdditionalFiles`. Spec errors are reported as build error
  `CSLEX001` at the offending line.

  ```xml
  <ItemGroup>
    <PackageReference Include="CsLex.SourceGenerator" Version="2.0.0-preview.1" PrivateAssets="all" />
    <AdditionalFiles Include="mylexer.lex" />
  </ItemGroup>
  ```

- **Library.** In the `CsLex` package:

  ```csharp
  string source = LexLib.LexGenerator.Generate(File.ReadAllText("mylexer.lex"));
  ```

  An invalid specification throws `LexSpecException`. `TryGenerate` returns a
  `LexResult` instead, with either the source or every error found. Parsing
  recovers at each declaration line and each rule, so one bad rule doesn't hide
  the next. The CLI and the source generator report all the errors.

The packages are built by `dotnet pack` but are not yet published to NuGet.

Generated lexers have no runtime dependency. By default the alphabet is 7-bit;
`%full` selects 8-bit, and `%unicode` selects all UTF-16 code units. A character
outside the alphabet is reported as unmatched input.

## Build and test

You need the .NET 10 SDK.

    dotnet build CsLex.slnx
    dotnet test CsLex.slnx

There are two further checks, both run by CI on Linux:

    tools/original-reference/reproduce.sh   # token goldens vs the 1999 generator
    tools/package-smoke/run.sh              # pack, install, and use the packages

`LexTests/Golden/*.generated.cs.txt` pins the generator's output. After an
intended change, rerun the tests with `CSLEX_UPDATE_GOLDEN=1` and review the
diff. The `*.tokens.txt` goldens are different: they come from the original
1999 generator and are only rewritten by `reproduce.sh --update`.

## Layout

| Path | Contents |
|---|---|
| `LexLib/` | The generator, with `LexGenerator` as its public entry point (package `CsLex`) |
| `CsLexConsole/` | The `cslex` command-line tool (package `CsLex.Tool`) |
| `CsLex.SourceGenerator/` | The Roslyn incremental generator (package `CsLex.SourceGenerator`) |
| `CsLexLib/` | Experimental runtime base class `CsLex<T>`; the generator does not emit code for it yet |
| `CsLexLibTest/` | A lexer for `simple.lex` written by hand against `CsLex<T>` |
| `LexTests/` | xUnit tests, sample specs and golden files |
| `tools/` | The reference and packaging checks |
| `docs/MODERNIZATION.md` | The 2026 review, the modernization plan, and its status |

## History

CsLex was written in 1999, when C# and .NET were new, and built with `nmake`.
That source is preserved at tag `original-1999`. In 2015 it was moved to a
Visual Studio solution and generic collections. In 2026 it was moved to
SDK-style projects on .NET 10, characterization tests were added, a number of
bugs were fixed, and Unicode support, packaging and a source generator were
added. See [docs/MODERNIZATION.md](docs/MODERNIZATION.md).

## License

MIT; see [LICENSE](LICENSE). CsLex is derived from JLex. See
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for its notice.
