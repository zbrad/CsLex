using System;
using System.IO;
using LexLib;

namespace CsLexConsole
{
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
            Console.WriteLine("Creating output file [" + outfile + "]");

            try
            {
                using (var input = new StreamReader(filename))
                using (var output = new StreamWriter(outfile))
                {
                    LexGenerator.Generate(input, output, Console.Out);
                }
            }
            catch (LexSpecException e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
    }
}
