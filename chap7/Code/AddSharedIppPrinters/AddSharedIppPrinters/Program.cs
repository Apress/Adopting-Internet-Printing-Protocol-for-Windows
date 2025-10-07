using AddSharedIppPrinters.Helpers;
using AddSharedIppPrinters.Print;
using System;
using System.Collections.Generic;
using System.Text;

namespace AddSharedIppPrinters
{
    public class Program
    {
        static CLProcessor _clp = CLProcessor.GetInstance;
        static string server = string.Empty;
        static string default_printer;
        static string _thisComputer = Environment.MachineName;
        static PrinterConnections _printerConnections = new PrinterConnections();
        static void Main(string[] args)
        {
            //Process the command line args..
            try
            {
                _clp.GetCommandLineArgs(args);
                server = _clp.PrintServer;
                ConsoleManager.WriteBlue(string.Format("Using print server {0} - retrieved from console argument /ps", server));
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteRed("Error retrieving command line parameters, reason: " + ex.Message);
                Usage();
                CloseProgram();
            }

            //Get the default printer
            int len = MiniPrintLib.SHARED_PRINTER_NAME_LENGTH;
            StringBuilder sb = new StringBuilder(string.Empty, len);
            try
            {
                MiniPrintLib.GetDefaultPrinterWrapper(ref sb, ref len);
                default_printer = sb.ToString();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteRed("Error retrieving the default printer, Win32 error: " + e.Message);
            }

            //Enumerate shared printers off a server...
            try
            {
                MiniPrintLib.ServerEnumPrinters(server, ref _printerConnections);
                {
                    ConsoleManager.WriteYellow("Enumerating all shared network printers on: " + server);
                    foreach (PrinterConnection printer in _printerConnections)
                    {
                        ConsoleManager.WriteDefault(printer.ToString());
                    }
                }

                //Since we have successfully enumerated printers from the server, we can purge
                //existing server based printers if desired...
                try
                {
                    List<string> local_printers = new List<string>();
                    local_printers = MiniPrintLib.LocalEnumPrinters(MiniPrintLib.PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS);

                    //Since we have successfully enumerated the shared printers off the server, we can remove them..
                    if (_clp.GetOptions(CLProcessor.PURGE) == true)
                    {
                        ConsoleManager.WriteYellow(string.Format("Removing existing print connections on {0} that were shared off {1}.", _thisComputer, server));
                        foreach (string printConnection in local_printers)
                        {
                            MiniPrintLib.DeletePrinterConnectionWrapper(printConnection);
                            ConsoleManager.WriteGreen("Removed print connection: " + printConnection);
                        }
                    }

                    //Ready to add new print connections from the server
                    ConsoleManager.WriteYellow("Adding print connections enumerated from: " + server);

                    //Now lets add the server based IPP print connectons...
                    foreach (PrinterConnection _pc in _printerConnections)
                    {
                        try
                        {
                            if(Helpers.CStringMethods.Contains(_pc.DRIVERNAME, "IPP", StringComparison.OrdinalIgnoreCase))
                            {
                                if ((_clp.GetOptions(CLProcessor.PRINT_LOCATIONS) == true) && (_clp.IsMatchingLocation(_pc.LOCATION) == true))
                                {
                                    MiniPrintLib.AddPrinterConnectionWrapper(_pc.PRINTERNAME);
                                    ConsoleManager.WriteGreen($"Added IPP print connection: {_pc.PRINTERNAME} because of Location field match on {_pc.LOCATION}");
                                }
                                else if (_clp.GetOptions(CLProcessor.PRINT_LOCATIONS) == false)
                                {
                                    MiniPrintLib.AddPrinterConnectionWrapper(_pc.PRINTERNAME);
                                    ConsoleManager.WriteGreen("Added IPP print connection: " + _pc.PRINTERNAME);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleManager.WriteRed(string.Format("Error adding: {0}, reason: {1}", _pc.PRINTERNAME, ex.Message));
                        }
                    }

                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteRed("Error enumerating local printers, reason: " + ex.Message);
                }

            }
            catch (Exception ex)
            {
                ConsoleManager.WriteRed("Error enumerating shared printers, reason: " + ex.Message);
            }
            ConsoleManager.WriteYellow("==============================================");
        }

        /// <summary>
        /// CloseProgram
        /// </summary>
        private static void CloseProgram()
        {
            ConsoleManager.ResetConsole();
            Environment.Exit(0);
        }

        private static void Usage()
        {
            ConsoleManager.ResetConsole();
            Console.WriteLine("===========AddSharedIppPrinters=============");
            Console.WriteLine("Usage: AddSharedIppPrinters </ps=print_server_name> </purge> </pl=loc1,loc2>");
            Console.WriteLine("All arguments except print server are optional, order is not important.");
            Console.WriteLine("AddSharedIppPrinters adds shared IPP printers from a print server to the local environment.");
            Console.WriteLine("Specify a print server by using the /ps=print_server_name argument where print_server_name");
            Console.WriteLine("is a string denoting the name of the print server name to use.");
            Console.WriteLine("The /purge argument removes existing print server connections from the local machine");
            Console.WriteLine("before any new shared printers (from the server) are added.");
            Console.WriteLine("You can filter IPP print connections by location string if desired");
            Console.WriteLine("To do so use the /pl argument with comma-delimted location strings");
            Console.WriteLine("The utility will then add only print connections whose location field");
            Console.WriteLine("matches one of the location strings");
            Console.WriteLine("");
        }

    }
}

