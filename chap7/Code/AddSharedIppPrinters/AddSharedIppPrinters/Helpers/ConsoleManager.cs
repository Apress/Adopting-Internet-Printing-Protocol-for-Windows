using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddSharedIppPrinters.Helpers
{
    /// <summary>
    /// ConsoleManager
    /// 
    /// Static class for basic manipulations to console output text color.
    /// </summary>
    public static class ConsoleManager
    {
        public static void ResetConsole()
        {
            Console.ResetColor();
        }

        public static void WriteGreen(string text) 
        { 
            Console.ForegroundColor = ConsoleColor.Green;   
            Console.WriteLine(text);
        }

        public static void WriteRed(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
        }

        public static void WriteYellow(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
        }

        public static void WriteBlue(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
        }

        public static void WriteDefault(string text) 
        {
            ResetConsole();
            Console.WriteLine(text);
        }
    }
}
