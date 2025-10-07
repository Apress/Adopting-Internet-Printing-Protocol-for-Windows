using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace CS_Multi_Check
{
    class Program
    {
        private const int npos = -1;
        static string printerList = null;
        static string[] printerNames;
        static List<string> printerNamesList = new List<string>();
        static async Task Main(string[] args)
        {
            try 
            {
                GetCommandLineArgs(args);
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString());
                return;
            }   
           

            ConcurrentBag<List<string>> cbPrinters = new ConcurrentBag<List<string>>();
            ConcurrentBag<string> cbErrors = new ConcurrentBag<string>();

            // Call the synchronous PInvoke function asynchronously using a foreach loop

            foreach (var printerName in printerNames)
            {
                try
                {
                    List<string> lstAttributes = await WinIppPinvokeMethods._getPrinterAttributes(printerName, Environment.UserName, (uint)Defines.HTTP_ENCRYPTION.HTTP_ENCRYPTION_IF_REQUESTED);
                    cbPrinters.Add(lstAttributes);
                }
                catch (Exception ex)
                {
                    cbErrors.Add($"GetPrinterAttributes failed for {printerName}, reason: {ex.Message}");
                }
            }


            // Print the results
            foreach (var printer in cbPrinters)
            {
                List <string> attrs = FormatOutput(printer);
                foreach (string attr in attrs)
                {
                    Console.WriteLine(attr);
                }
            }

            if(cbErrors.Count() > 0)
            {
                Console.WriteLine("Notice! - The following errors were recorded..");
                foreach (string strError in cbErrors)
                {
                    Console.WriteLine($"{strError}");
                }
            }
            Console.WriteLine("----fini----");
        }

        static List<string> FormatOutput(List<string> retVals)
        {
            List<string> newList = new List<string>();
            foreach (string retLine in retVals)
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
                    foreach (string s in fs)
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

        /// <summary>
        /// GetCommandLineArgs
        /// 
        /// Get the printer names list or throw an exception
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        static void GetCommandLineArgs(string[] args)
        {
            int count = 0;
            if (args.Length != 1)
            {
                throw new Exception("Invalid command line arguments count!");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/l=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        printerList = args[i].Substring(3);
                        count++;
                    }
                }
                if (count != 1)
                {
                    throw new Exception("Processing of command line failed, invalid arguments");
                }
                else
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(printerList);
                        foreach (string line in lines) 
                        {
                            if(line.Length > 0) 
                            { 
                                printerNamesList.Add(line.Trim()); 
                            }
                        }
                        printerNames = printerNamesList.ToArray();
                        return;
                    }
                    catch (Exception ex) 
                    {
                        throw new Exception($"Processing of printers file {printerList} failed, reason: {ex.Message}");
                    }
                }
            }
        }

    }
}


