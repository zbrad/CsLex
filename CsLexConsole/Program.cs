using System;
using System.IO;
using LexLib;

namespace CsLexConsole;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("usage: cslex <filename>");
            return 2;
        }

        var filename = args[0];
        if (!File.Exists(filename))
        {
            Console.WriteLine("Error: Unable to open input file " + filename + ".");
            return 1;
        }

        var outfile = Path.GetFileNameWithoutExtension(filename) + ".cs";

        LexResult result;
        using (var input = new StreamReader(filename))
            result = LexGenerator.TryGenerate(input, Console.Out);
        if (!result.Success)
        {
            foreach (var d in result.Diagnostics)
                Console.WriteLine(d.Message);
            return 1;
        }

        // Written only on success, so a bad spec doesn't leave a stale or empty file.
        Console.WriteLine("Creating output file [" + outfile + "]");
        File.WriteAllText(outfile, result.Source);
        return 0;
    }
}
