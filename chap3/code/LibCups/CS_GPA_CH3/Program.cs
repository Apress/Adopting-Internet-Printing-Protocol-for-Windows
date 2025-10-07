using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace CS_GPA_CH3
{
    internal class Program
    {
        private const int npos = -1;
        static void Main(string[] args)
        {
            Console.Write("IPP printer name? ");
            string printer_name = Console.ReadLine();
            List<string> attrs = new List<string>();
            try
            {
                attrs = FormatOutput(WinIppPinvokeMethods._getPrinterAttributes(printer_name, Environment.UserName, (uint)Defines.HTTP_ENCRYPTION.HTTP_ENCRYPTION_IF_REQUESTED));
                foreach (string attr in attrs) 
                { 
                    Console.WriteLine(attr);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error performing get-printer-attributes test: {ex.Message}");  
            }

            Console.WriteLine("fini");
        }


        /// <summary>
        /// FormatOutput
        /// </summary>
        /// <param name="retVals"></param>
        /// <returns></returns>
        static List<string> FormatOutput(List<string> retVals)
        {
            List<string> newList = new List<string>();  
            foreach(string retLine in retVals)
            {
                string[] ret = retLine.Split('|');
                if (ret[1].IndexOf('{') == npos)
                {
                    newList.Add(ret[0] + ':');
                    string[] vs = ret[1].Split(',');
                    foreach (string s in vs)
                    {
                        if (s.Length > 0)
                        {
                            newList.Add('\t' + s);
                        }
                    }
                }
                else
                {
                    newList.Add(ret[0] + ':');
                    int len = ret[1].Length;
                    string s1 = ret[1].Replace('{', ' ');
                    string s2 = s1.Replace('}', ',');
                    string[] fs = s2.Split(',');
                    foreach(string s in fs) 
                    {
                        if (s.Length > 0)
                        {
                            newList.Add('\t' + s);
                        }
                    }
                }
            }
            return newList;
        }

    }
}
