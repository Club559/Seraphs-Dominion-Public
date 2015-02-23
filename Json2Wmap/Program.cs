using System;
using System.IO;

namespace Json2Wmap
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("usage: Json2wmapConv.exe jsonfile wmapfile");
                return;
            }
            try
            {
                var dta = new XmlData();
                var fi = new FileInfo(args[0]);
                if (fi.Exists)
                    terrain.Json2Wmap.Convert(dta, args[0], args[1]);
                else
                {
                    Console.WriteLine("input file not found: " + fi.FullName);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e);
            }
            Console.WriteLine("done");
        }
    }
}