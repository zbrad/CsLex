using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexLib;
using System.IO;

namespace CsLexConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Gen lg;

            if (args.Length < 2)
            {
                Console.WriteLine("lex <filename>");
                return;
            }

            var filename = args[1];
            if (!File.Exists(filename))
            {
                Console.WriteLine("Error: Unable to open input file " + filename + ".");
                return;
            }

            var instream = new StreamReader(
                            new FileStream(filename, FileMode.Open,
                                   FileAccess.Read, FileShare.Read,
                                   Properties.Settings.Default.MaxBuf)
                            );
            if (instream == null)
            {
                Console.WriteLine("Error: Unable to open input file " + filename + ".");
                return;
            }

            var outfile = Path.GetFileNameWithoutExtension(filename) + ".cs";

            Console.WriteLine("Creating output file [" + outfile + "]");

            var outstream = new StreamWriter(
                         new FileStream(outfile,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.Write, 8192)
                         );

            lg = new Gen(instream, outstream);
            lg.generate();
        }
    }
}
