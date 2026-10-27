using System.IO;

namespace LexLib
{
    /// <summary>
    /// Where the generator's progress and debug output goes. Gen.generate()
    /// sets it for the duration of a run (per thread); outside a run, and by
    /// default, output is discarded.
    /// </summary>
    internal static class Log
    {
        [System.ThreadStatic]
        private static TextWriter current;

        public static TextWriter Out => current ?? TextWriter.Null;

        /// <summary>Install a writer; returns the previous one for restoring.</summary>
        public static TextWriter Swap(TextWriter writer)
        {
            var previous = current;
            current = writer;
            return previous;
        }
    }
}
