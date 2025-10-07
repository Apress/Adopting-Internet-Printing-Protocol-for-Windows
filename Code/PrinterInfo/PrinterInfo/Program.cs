using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;
using System.Net;
using System.IO;

namespace PrinterInfo
{
    class Program
    {
        private const string MIB_2707 = "1.3.6.1.4.1.2699.1.2";
        private static string _sPrintersList = string.Empty;

        static void Main(string[] args)
        {

            CPrinterInfoCollection _cpic = new CPrinterInfoCollection();

            try
            {
                GetCommandLineArgs(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error processing command line args, reason: {ex.Message}");
                Help();
                return;
            }


            try
            {
                using (StreamReader sr = new StreamReader(_sPrintersList))
                {
                    while (sr.Peek() >= 0)
                    {
                        _cpic.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not find file {_sPrintersList}, reason: {ex.Message}");
                return;
            }


             foreach (CPrinterInfo _cpi in _cpic)
             {
                 
                 Console.WriteLine("============" + _cpi.IP_ADDRESS_STRING + "=============");
                 CGenMib gm = new CGenMib(400, _cpi.COMMUNITY_STRING);
                 try
                 {
                     gm.GetInfo(_cpi.IP_ADDRESS_STRING);
                 }
                 catch (Exception e1)
                 {
                     Console.WriteLine("Exception returning results for MIB2: " + e1.Message);
                 }
                 gm.GetValues();
                 gm.PrintValues();

                //---If the call below fails, try increasing the C3805Mib Maximum Values--
                C3805Mib pm = new C3805Mib(600, _cpi.COMMUNITY_STRING);
                 try
                 {
                     pm.GetInfo(_cpi.IP_ADDRESS_STRING);
                 }
                 catch (Exception e)
                 {
                     Console.WriteLine("Exception returning results for PRINTER_MIB: " + e.Message);
                 }
                 pm.GetValues();
                 pm.PrintValues();
              
                 Console.WriteLine("===================================================");
             }

            Console.WriteLine("fini");
        }

        /// <summary>
        /// Help
        /// </summary>
        static void Help()
        {
            Console.WriteLine("Usage: PrinterInfo /p=<printer_file>,<ommunity string>");
            Console.WriteLine("<printer_File> Format is: <ip_address>,<community_string>");
            Console.WriteLine("One entry per line, example: 106.23.241.13,public");
            Console.WriteLine("Both path and filename should be specified for printers list");
            return;
        }

        /// <summary>
        /// GetCommandLineArgs
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
                    if (args[i].StartsWith("/p=", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _sPrintersList= args[i].Substring(3);
                        count++;
                    }
                }
                if ((_sPrintersList.Length==0) && (count !=1)) 
                {
                    throw new Exception("Processing of command line failed, invalid arguments");
                }
            }
        }

    }
}