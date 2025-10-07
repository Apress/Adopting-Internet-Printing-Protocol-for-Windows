using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace AddIppPrinter
{
    /// <summary>
    /// Program to add an ipp printer to workstation using printer ip address
    /// 
    /// Note: The name of the printer added is a product of the IPP get-printer-attributes
    /// query performed when the WSD port is added to the computer. The IPP printer added
    /// is by default not shared.
    /// </summary>
    internal class Program
    {
        const int ERROR_SUCCESS = 0;
        static string sIppPrinter;
        static string sPort;
        static string sList;
        static bool bList = false;
        static List<string> lstPrinters = new List<string>();

        public enum IPP_TYPE
        {
            IPP = 0,
            IPPS = 1
        };

        //-----------------PInvoke ----------------------
        //--------Printer Access Values-----------
        private const int SERVER_ACCESS_ADMINISTRATOR = 0x1;
        private const int SERVER_ACCESS_ENUMERATE = 0x2;
        private const int SERVER_ALL_ACCESS = 0x3;
        private const int PRINTER_ACCESS_ADMINISTRATOR = 0x4;
        private const int PRINTER_ACCESS_USE = 0x8;
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int PRINTER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | PRINTER_ACCESS_ADMINISTRATOR | PRINTER_ACCESS_USE;


        //Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct PRINTER_DEFAULTS
        {
            public IntPtr pDatatype;
            public IntPtr pDevMode;
            public int DesiredAccess;
        }


        // Import necessary functions from winspool.drv
        [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool OpenPrinterW([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        // Import the XcvData function from winspool.drv
        [DllImport("winspool.drv", EntryPoint = "XcvDataW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool XcvDataW(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDataName, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pIppPrinter, uint cbInputData, IntPtr pOutputData, uint cbOutputData, out uint pcbOutputNeeded, out uint pdwStatus);

        //-----------------PInvoke ----------------------


        static void Main(string[] args)
        {
            try
            {
                CheckCommandLine(args);
            } 
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                PrintUsage();
                return;
            }

            if(bList == true)
            {
                foreach(string p in lstPrinters)
                {
                    try
                    {
                        AddIppPrinter(sPort == "631"? IPP_TYPE.IPP: IPP_TYPE.IPPS, p);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    AddIppPrinter(sPort == "631"? IPP_TYPE.IPP : IPP_TYPE.IPPS, sIppPrinter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

       
        /// <summary>
        /// AddIppPrinter
        /// 
        /// Adds an IPP printer to the local machine. This call creates the WSD port and the printer
        /// itself. 
        /// </summary>
        /// <param name="type">IPP or IPPS</param>
        /// <param name="sIpAddress">The IP address of the printer to add</param>
        /// 
        /// <exception cref="Exception"></exception>
        /// Note: This method throws exceptions, caller should wrap it in a try/catch block.
        public static void AddIppPrinter(IPP_TYPE type, string sIpAddress)
        {

            string sPrinterAddress = string.Empty;
            switch (type)
            {
                case IPP_TYPE.IPP:
                    sPrinterAddress = @"ipp://" + sIpAddress + @":631/ipp/print";
                    break;
                case IPP_TYPE.IPPS:
                    sPrinterAddress = @"ipps://" + sIpAddress + @":443/ipp/print";
                    break;
            }

            try
            {
                AddIppPrinter(sPrinterAddress);
                if (type == IPP_TYPE.IPP )
                    Console.WriteLine("Success: IPP printer created from {0}", sIpAddress);
                else
                    Console.WriteLine("Success: IPPS printer created from {0}", sIpAddress);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Attempt to create an IPP/IPPS printer from {0} failed, reason: {1}", sIpAddress, ex.Message);
            }
        }

        /// <summary>
        /// CheckCommandLine
        /// Process the command line created by the caller. Caller can stipulate:
        /// 
        /// 
        /// A lone printer with switch /p, to which an IP address of a printer must be supplied.
        /// OR
        /// A list of ip addresses /l, to which a file must be supplied.
        /// 
        /// The caller must stipulate port 631 or 443 via the /s switch.
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static void CheckCommandLine(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                throw new Exception("Invalid number of command line arguments!");
            }
            else
            {
                for(int i = 0; i < args.Length; i++)
                {
                    try
                    {
                        string sSwitch = args[i].ToString().Substring(0, 2).Trim();
                        if (sSwitch == "/p")
                        {
                            sIppPrinter = args[i].ToString().Substring(3).Trim();
                        }
                        else if (sSwitch == "/l")
                        {
                            sList = args[i].ToString().Substring(3).Trim();
                            ProcessList(sList);
                            bList = true;
                        }
                        else if (sSwitch == "/s")
                        {
                            sPort = args[i].ToString().Substring(3).Trim();
                            if((sPort != "631")&&(sPort != "443"))
                            {
                                throw new Exception("Invalid IPP port number provided using the /s switch - only 443/631 allowed!");
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid command line switch provided!");
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Error processing command line!");
                    }
                }
            }
        }


        /// <summary>
        /// ProcessList
        /// </summary>
        /// <param name="sList"></param>
        private static void ProcessList(string sList)
        {
            try
            {
                string[] tempList = File.ReadAllLines(sList);
                foreach (string s in tempList)
                {
                    if(s.Trim().Length != 0)
                    {
                        lstPrinters.Add(s); 
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening or reading list of printers: " + e.ToString());
            }
        }
        
        /// <summary>
        /// PrintUsage
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("---AddIppPrinter---");
            Console.WriteLine("Usage: AddIppPrinter /p=<printer IP address> /s=<631 or 443>");
            Console.WriteLine("Adds an IPP/IPPS printer via IP address");
            Console.WriteLine("Usage: AddIppPrinter /l=<text file of ip addresses> /s=<631 or 443>");
            Console.WriteLine("Opens the list and adds the IPP/IPPS printers based on IP addresses");
            Console.WriteLine("The mandatory /s switch specifies IPP (port 631) or IPPS (port 443)");
            Console.WriteLine("If 631 is chosen and the printer supports IPPS, STARTTLS will be employed");
            Console.WriteLine("to ensure TLS is used.");
            Console.WriteLine("---------------");
        }
     

        /// <summary>
        /// AddIppPrinter
        /// 
        /// Private method to create an IPP Printer
        /// </summary>
        /// <param name="sIppPrinterUrl"></param>
        /// <exception cref="Exception"></exception>
        private static void AddIppPrinter(string sIppPrinterUrl)
        {
            IntPtr hXcv = IntPtr.Zero;
            StringBuilder sbMonitorName = new StringBuilder(",XcvMonitor WSD Port");
            StringBuilder sbDataName = new StringBuilder("AssocIppDirected");

            PRINTER_DEFAULTS pDefaults = new PRINTER_DEFAULTS
            {
                pDatatype = IntPtr.Zero,
                pDevMode = IntPtr.Zero,
                DesiredAccess = SERVER_ACCESS_ADMINISTRATOR
            };

            try
            {
                // Open a handle to the printer
                if (!OpenPrinterW(sbMonitorName, out hXcv, ref pDefaults))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Exception("OpenPrinter API Exception, Win32 Error: " + lastWin32Error.ToString());
                }

                StringBuilder sbIpp = new StringBuilder(sIppPrinterUrl);
                uint pcbOutputNeeded, uiStatus;
                uint uiSizeIppPrinterString = (uint)(sizeof(char) * (sIppPrinterUrl.Length + 1));

                if (!XcvDataW(hXcv, sbDataName, sbIpp, uiSizeIppPrinterString, IntPtr.Zero, 0, out pcbOutputNeeded, out uiStatus))
                {
                    throw new Exception("Unknown error - XcvDataW API");
                }
                if (uiStatus != ERROR_SUCCESS)
                {
                    if(uiStatus == 16)
                    {
                        throw new Exception(string.Format("XcvDataW API failed, status error: {0}, this usually means WSD port already exists.", uiStatus));
                    }
                    else 
                    {
                        throw new Exception("XcvDataW API failed, status error: " + uiStatus);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Ipp/Ipps Port addition failed, reason: " + ex.Message);
            }
            finally
            {
                // Close the printer handle
                if (hXcv != IntPtr.Zero)
                {
                    ClosePrinter(hXcv);
                }
            }
        }
    }
}
