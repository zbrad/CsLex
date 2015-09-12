using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexLib;

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

            lg = new Gen(args[1]);
            lg.generate();
        }
    }
}
