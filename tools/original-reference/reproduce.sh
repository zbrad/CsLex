#!/usr/bin/env bash
# Rebuild the original 1999 generator (lex/src at tag original-1999) on the
# current .NET SDK, generate lexers for the sample specs, run them on
# test.txt, and compare with the token goldens in LexTests/Golden.
#
# Usage: tools/original-reference/reproduce.sh [--update]
#   --update  copy the reference output over LexTests/Golden/*.tokens.txt
#
# Needs: git, dotnet (net10.0 SDK). Exit status is non-zero on a mismatch.
set -euo pipefail

here=$(cd "$(dirname "$0")" && pwd)
repo=$(git -C "$here" rev-parse --show-toplevel)
ref=original-1999
work=$(mktemp -d)
echo "work dir: $work"

git -C "$repo" archive "$ref" lex | tar -x -C "$work"
mkdir -p "$work/gen"
cp "$work"/lex/src/*.cs "$work/gen/"
cp "$here/project.csproj.txt" "$work/gen/gen.csproj"
dotnet build "$work/gen" -c Release -o "$work/gen/out" --nologo -v q >/dev/null

status=0
for spec in simple sample; do
    mkdir -p "$work/$spec"
    cp "$work/lex/tests/$spec.lex" "$work/$spec/"
    # The original reads Environment.GetCommandLineArgs()[1] and writes
    # <name>_lex.cs into the current directory.
    (cd "$work/$spec" && dotnet "$work/gen/out/gen.dll" "$spec.lex" >/dev/null)
    cp "$here/project.csproj.txt" "$work/$spec/run.csproj"
    dotnet build "$work/$spec/run.csproj" -c Release -o "$work/$spec/out" --nologo -v q >/dev/null
    dotnet "$work/$spec/out/run.dll" "$work/lex/tests/test.txt" > "$work/$spec.tokens.txt"

    golden="$repo/LexTests/Golden/$spec.tokens.txt"
    if [[ "${1:-}" == "--update" ]]; then
        cp "$work/$spec.tokens.txt" "$golden"
        echo "updated $golden"
    elif diff -u "$golden" "$work/$spec.tokens.txt"; then
        echo "$spec: matches golden"
    else
        echo "$spec: DIFFERS from golden"
        status=1
    fi
done
exit $status
