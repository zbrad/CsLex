#!/usr/bin/env bash
# Pack CsLex.Tool and CsLex.SourceGenerator, then use each from a fresh
# directory the way a consumer would, and check the generated lexer's output
# against the 1999 reference (LexTests/Golden/simple.tokens.txt).
#
# Usage: tools/package-smoke/run.sh
# Needs: dotnet (net10.0 SDK). Nothing is published; packages stay in a temp dir.
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
repo=$(git -C "$here" rev-parse --show-toplevel)
work=$(mktemp -d)
echo "work dir: $work"
golden="$repo/LexTests/Golden/simple.tokens.txt"

dotnet pack "$repo/CsLexConsole" -c Release -o "$work/pkgs" --nologo -v q >/dev/null
dotnet pack "$repo/CsLex.SourceGenerator" -c Release -o "$work/pkgs" --nologo -v q >/dev/null
version=$(ls "$work/pkgs" | sed -n 's/^CsLex\.Tool\.\(.*\)\.nupkg$/\1/p')

# 1. The dotnet tool.
dotnet tool install CsLex.Tool --version "$version" --tool-path "$work/tool" \
    --add-source "$work/pkgs" >/dev/null
mkdir -p "$work/cli"
cp "$repo/LexTests/LexFiles/simple.lex" "$work/cli/"
(cd "$work/cli" && "$work/tool/cslex" simple.lex >/dev/null)
test -s "$work/cli/simple.cs"
echo "tool: generated simple.cs"

# 2. The source generator, in a strict (nullable, warnings-as-errors) project.
mkdir -p "$work/app"
cp "$repo/LexTests/LexFiles/simple.lex" "$repo/LexTests/LexFiles/test.txt" "$work/app/"
sed "s|@VERSION@|$version|" "$here/consumer.csproj.txt" > "$work/app/app.csproj"
cat > "$work/app/nuget.config" <<XML
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$work/pkgs" />
  </packageSources>
</configuration>
XML
dotnet build "$work/app" -c Release -o "$work/app/out" --nologo -v q >/dev/null
dotnet "$work/app/out/app.dll" "$work/app/test.txt" > "$work/app.tokens.txt"
diff -u "$golden" "$work/app.tokens.txt"
echo "source generator: output matches $golden"
