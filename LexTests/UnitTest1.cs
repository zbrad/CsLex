using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LexLib;
using System.Reflection;
using System.IO;
using System.Text;

namespace LexTests
{
    [TestClass]
    public class UnitTest1
    {
        

        [TestMethod]
        public void TestMethod1()
        {
            
            var input = GetInputFile("simple.lex");
            var outwriter = new StringWriter();
            var outmem = new MemoryStream();
            var output = new StreamWriter(outmem);
            var lg = new Gen(input, output);
            lg.generate();
            outmem.Position = 0;
            var outstream = new StreamReader(outmem);
            string outstring = outstream.ReadToEnd();

        }

        public static StreamReader GetInputFile(string filename)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            string path = "LexTests.LexFiles";

            foreach (var v in thisAssembly.GetManifestResourceNames())
                Console.WriteLine(v);

            var resource = thisAssembly.GetManifestResourceStream(path + "." + filename);
            return new StreamReader(resource);
        }
    }
}
