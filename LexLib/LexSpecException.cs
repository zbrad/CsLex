using System;

#nullable enable

namespace LexLib;

/// <summary>
/// Thrown when a lex specification cannot be processed. Replaces the
/// original behaviour of printing the error and exiting the process.
/// </summary>
public class LexSpecException : Exception
{
    /// <summary>One of the Error.E_* codes, or -1 when there is none.</summary>
    public int ErrorCode { get; }

    /// <summary>1-based line in the specification, or 0 when unknown.</summary>
    public int Line { get; }

    public LexSpecException(int errorCode, int line, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        Line = line;
    }
}
