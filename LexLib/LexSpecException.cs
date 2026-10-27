using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace LexLib;

/// <summary>
/// Thrown when a lex specification cannot be processed. Replaces the
/// original behaviour of printing the error and exiting the process.
/// ErrorCode, Line and Message describe the first error; Diagnostics lists
/// every error found.
/// </summary>
#if CSLEX_EMBEDDED
internal
#else
public
#endif
class LexSpecException : Exception
{
    /// <summary>One of the Error.E_* codes, or -1 when there is none.</summary>
    public int ErrorCode { get; }

    /// <summary>1-based line in the specification, or 0 when unknown.</summary>
    public int Line { get; }

    /// <summary>Every error found, in input order; the first is this one.</summary>
    public IReadOnlyList<LexDiagnostic> Diagnostics { get; }

    public LexSpecException(int errorCode, int line, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        Line = line;
        Diagnostics = new[] { new LexDiagnostic(errorCode, line, message) };
    }

    /// <summary>Several errors; the first one supplies code, line and message.</summary>
    public LexSpecException(IReadOnlyList<LexDiagnostic> diagnostics)
        : base(Summary(diagnostics))
    {
        ErrorCode = diagnostics[0].ErrorCode;
        Line = diagnostics[0].Line;
        Diagnostics = diagnostics.ToArray();
    }

    static string Summary(IReadOnlyList<LexDiagnostic> diagnostics)
    {
        if (diagnostics.Count == 0)
            throw new ArgumentException("At least one diagnostic is required.", nameof(diagnostics));
        return diagnostics.Count == 1
            ? diagnostics[0].Message
            : diagnostics[0].Message + "\n(and " + (diagnostics.Count - 1) + " more error"
                + (diagnostics.Count == 2 ? "" : "s") + ")";
    }
}
