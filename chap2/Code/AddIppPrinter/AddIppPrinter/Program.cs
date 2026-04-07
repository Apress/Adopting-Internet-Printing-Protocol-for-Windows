using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


internal class Program
{
    private const int ERROR_SUCCESS = 0;
    private static string sIppPrinter;
    private static string sList;
    private static bool bList = false;
    private static List<string> lstPrinters = new List<string>();
    private static Program.IPP_TYPE type;
    private const int SERVER_ACCESS_ADMINISTRATOR = 1;
    private const int SERVER_ACCESS_ENUMERATE = 2;
    private const int SERVER_ALL_ACCESS = 3;
    private const int PRINTER_ACCESS_ADMINISTRATOR = 4;
    private const int PRINTER_ACCESS_USE = 8;
    private const int STANDARD_RIGHTS_REQUIRED = 983040 /*0x0F0000*/;
    private const int PRINTER_ALL_ACCESS = 983052 /*0x0F000C*/;

    
    //-------------- pInvoke Interop defines ---------------------
    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool OpenPrinterW(
      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPrinterName,
      out IntPtr phPrinter,
      ref Program.PRINTER_DEFAULTS pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool XcvDataW(
      IntPtr handle,
      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pDataName,
      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pIppPrinter,
      uint cbInputData,
      IntPtr pOutputData,
      uint cbOutputData,
      out uint pcbOutputNeeded,
      out uint pdwStatus);

    //-------------- pInvoke Interop defines ---------------------

    public enum IPP_TYPE
    {
        IPP,
        IPPS,
        IPPS_443,
    }

    private struct PRINTER_DEFAULTS
    {
        public IntPtr pDatatype;
        public IntPtr pDevMode;
        public int DesiredAccess;
    }

    private static void Main(string[] args)
    {
        try
        {
            Program.CheckCommandLine(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Program.PrintUsage();
            return;
        }
        if (Program.bList)
        {
            foreach (string lstPrinter in Program.lstPrinters)
            {
                try
                {
                    Program.AddIppPrinter(Program.type, lstPrinter);
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
                Program.AddIppPrinter(Program.type, Program.sIppPrinter);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// AddIppPrinter
    /// </summary>
    /// <param name="type"></param>
    /// <param name="sIpAddress"></param>
    public static void AddIppPrinter(Program.IPP_TYPE type, string sIpAddress)
    {
        string sIppPrinterUrl = string.Empty;
        switch (type)
        {
            case Program.IPP_TYPE.IPP:
                sIppPrinterUrl = $"ipp://{sIpAddress}/ipp/print";
                break;
            case Program.IPP_TYPE.IPPS:
                sIppPrinterUrl = $"ipps://{sIpAddress}/ipp/print";
                break;
            case Program.IPP_TYPE.IPPS_443:
                sIppPrinterUrl = $"ipps://{sIpAddress}:443/ipp/print";
                break;
        }
        try
        {
            Program.AddIppPrinter(sIppPrinterUrl);
            switch (type)
            {
                case Program.IPP_TYPE.IPP:
                    Console.WriteLine("Success: IPP printer created from {0}", (object)sIpAddress);
                    break;
                case Program.IPP_TYPE.IPPS:
                    Console.WriteLine("Success: IPPS printer created from {0}", (object)sIpAddress);
                    break;
                case Program.IPP_TYPE.IPPS_443:
                    Console.WriteLine("Success: IPPS printer on port 443 created from {0}", (object)sIpAddress);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Attempt to create an IPP/IPPS printer from {0} failed, reason: {1}", (object)sIpAddress, (object)ex.Message);
        }
    }

    /// <summary>
    /// CheckCommandLine
    /// </summary>
    /// <param name="args"></param>
    /// <exception cref="Exception"></exception>
    private static void CheckCommandLine(string[] args)
    {
        if (args.Length != 2)
        {
            Program.PrintUsage();
            throw new Exception("Invalid number of command line arguments!");
        }
        for (int index = 0; index < args.Length; ++index)
        {
            try
            {
                switch (args[index].ToString().Substring(0, 2).Trim())
                {
                    case "/p":
                        Program.sIppPrinter = args[index].ToString().Substring(3).Trim();
                        continue;
                    case "/l":
                        Program.sList = args[index].ToString().Substring(3).Trim();
                        Program.ProcessList(Program.sList);
                        Program.bList = true;
                        continue;
                    case "/s":
                        string lower = args[index].ToString().Substring(3).Trim().ToLower();
                        if (lower != "ipp" && lower != "ipps" && lower != "ipps_443")
                            throw new Exception("Invalid IPP port number provided using the /s switch - choose ipp, ipps, or ipps_443!");
                        switch (lower)
                        {
                            case "ipp":
                                Program.type = Program.IPP_TYPE.IPP;
                                continue;
                            case "ipps":
                                Program.type = Program.IPP_TYPE.IPPS;
                                continue;
                            default:
                                Program.type = Program.IPP_TYPE.IPPS_443;
                                continue;
                        }
                    default:
                        throw new Exception("Invalid command line switch provided!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing command line!");
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
            foreach (string readAllLine in File.ReadAllLines(sList))
            {
                if (readAllLine.Trim().Length != 0)
                    Program.lstPrinters.Add(readAllLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening or reading list of printers: " + ex.ToString());
        }
    }


    /// <summary>
    /// PrintUsage
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("---AddIppPrinter---");
        Console.WriteLine("Usage: AddIppPrinter /p=<printer IP address> /s=<ipp or ipps or ipps_443>");
        Console.WriteLine("Adds the specified IPP/IPPS printer via printer name or IP Address");
        Console.WriteLine("The mandatory /s switch specifies whether IPP or IPPS is used");
        Console.WriteLine("");
        Console.WriteLine("Usage: AddIppPrinter /l=<text file of ip addresses or printer names> /s=<ipp/ipps/ipps_443>");
        Console.WriteLine("Opens the list and adds the IPP/IPPS printers based on IP addresses");
        Console.WriteLine("The mandatory /s switch specifies whether IPP or IPPS is used");
        Console.WriteLine("");
        Console.WriteLine("Typically on Windows, either ipp or ipps should be used. Optionally,");
        Console.WriteLine("you may elect IPPS on port 443 as this is the port HTTPS uses and is");
        Console.WriteLine("likely open on firewalls. After WPP is enabled, best practice would");
        Console.WriteLine("dictate using IPPS over IPP.");
        Console.WriteLine("");
        Console.WriteLine("Notice: If IPP and IPPS are enabled on the target printer, Windows will");
        Console.WriteLine("install an IPPS printer even if you specify IPP.");
        Console.WriteLine("");
        Console.WriteLine("---------------");
    }


    /// <summary>
    /// AddIppPrinter
    /// </summary>
    /// <param name="sIppPrinterUrl"></param>
    /// <exception cref="Exception"></exception>
    private static void AddIppPrinter(string sIppPrinterUrl)
    {
        IntPtr phPrinter = IntPtr.Zero;
        StringBuilder pszPrinterName = new StringBuilder(",XcvMonitor WSD Port");
        StringBuilder pDataName = new StringBuilder("AssocIppDirected");
        Program.PRINTER_DEFAULTS pDefault = new Program.PRINTER_DEFAULTS()
        {
            pDatatype = IntPtr.Zero,
            pDevMode = IntPtr.Zero,
            DesiredAccess = 1
        };
        try
        {
            if (!Program.OpenPrinterW(pszPrinterName, out phPrinter, ref pDefault))
                throw new Exception("OpenPrinter API Exception, Win32 Error: " + Marshal.GetLastWin32Error().ToString());
            StringBuilder pIppPrinter = new StringBuilder(sIppPrinterUrl);
            uint cbInputData = (uint)(2 * (sIppPrinterUrl.Length + 1));
            uint pdwStatus;
            if (!Program.XcvDataW(phPrinter, pDataName, pIppPrinter, cbInputData, IntPtr.Zero, 0U, out uint _, out pdwStatus))
                throw new Exception("Unknown error - XcvDataW API");
            if (pdwStatus == 0U)
                return;
            if (pdwStatus == 16U /*0x10*/)
                throw new Exception($"XcvDataW API failed, status error: {pdwStatus}, this usually means WSD port already exists.");
            throw new Exception("XcvDataW API failed, status error: " + pdwStatus.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Ipp/Ipps Port addition failed, reason: " + ex.Message);
        }
        finally
        {
            if (phPrinter != IntPtr.Zero)
                Program.ClosePrinter(phPrinter);
        }
    }

    
}