using System.Collections.Generic;

#nullable enable

namespace LexLib;

/// <summary>One error in a lex specification.</summary>
#if CSLEX_EMBEDDED
internal
#else
public
#endif
sealed class LexDiagnostic
{
    /// <summary>One of the Error.E_* codes, or -1 when there is none.</summary>
    public int ErrorCode { get; }

    /// <summary>1-based line in the specification, or 0 when unknown.</summary>
    public int Line { get; }

    public string Message { get; }

    public LexDiagnostic(int errorCode, int line, string message)
    {
        ErrorCode = errorCode;
        Line = line;
        Message = message;
    }

    public override string ToString() => Message;
}

/// <summary>Outcome of LexGenerator.TryGenerate.</summary>
#if CSLEX_EMBEDDED
internal
#else
public
#endif
sealed class LexResult
{
    /// <summary>The generated lexer, or null when the spec has errors.</summary>
    public string? Source { get; }

    /// <summary>Every error found; empty on success.</summary>
    public IReadOnlyList<LexDiagnostic> Diagnostics { get; }

    public bool Success => Source != null;

    internal LexResult(string? source, IReadOnlyList<LexDiagnostic> diagnostics)
    {
        Source = source;
        Diagnostics = diagnostics;
    }
}
