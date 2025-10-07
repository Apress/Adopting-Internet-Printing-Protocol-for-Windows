using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GetWindowsPortMonitors
{
    internal class Program
    {

        public static int ERROR_SUCCESS = 0;
        public static int ERROR_INSUFFICIENT_BUFFER = 122;
        private const string IPP_HIVE_REG = @"SYSTEM\ControlSet\Enum\SWD\IPP";
        private static string PORT_HIVE_REG = @"SYSTEM\ControlSet\Control\Print\Monitors\WSD Port\Ports";

        //------------------------------ InterOp --------------------------------------
        //PortType enum
        //struct for PORT_INFO_2
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PORT_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPortName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pMonitorName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDescription;
            public PortType fPortType;
            internal uint Reserved;
        }


        [Flags]
        public enum PortType
        {
            Write = 0x1,
            Read = 0x2,
            Redirected = 0x4,
            NetAttached = 0x8
        }

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnumPorts(string pName, uint level, IntPtr lpbPorts, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

        //------------------------------ InterOp --------------------------------------
        static void Main(string[] args)
        {
            List<CPrinterPort> pPorts = GetPrinterPorts("");
            string sPrinterUUID = string.Empty;
            string sPrinterName = string.Empty;
            string sDeviceDesc = string.Empty;
            string sWsdAddress = string.Empty;
            string sWsdPortGuid = string.Empty;

            foreach (CPrinterPort port in pPorts)
            {
                Console.WriteLine("\n");
                Console.WriteLine("Port Name: " + port.sPortName);
                sWsdPortGuid = port.sPortName;
                Console.WriteLine("Port Monitor: " + port.sMonitorName);
                Console.WriteLine("Port Description: " + port.sDescription);
                Console.WriteLine("Port Type: " + PrintPortType(port.bPortType));


                if (port.sDescription == "IPP Port")
                {
                    try
                    {
                        sPrinterUUID = GetPrinterUUIDFromPortName(port.sPortName);
                        Console.WriteLine("Associated Printer UUID: " + sPrinterUUID);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not recover printer uuid.");
                        sPrinterUUID = string.Empty;
                    }

                    try
                    {
                        string[] info = GetInfoFromPrinterUUID(sPrinterUUID);
                        sPrinterName = info[0];
                        sDeviceDesc = info[1];
                        Console.WriteLine("Printer Friendly Name: " + sPrinterName);
                        Console.WriteLine("Printer Device Description: " + sDeviceDesc);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not recover printer information.");
                    }

                    if (port.sDescription == "IPP Port")
                    {
                        try
                        {
                            string ip_address = GetARecord(sPrinterName);
                            Console.WriteLine("IP Address of printer on this port: " + ip_address);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Could not recover WSD Port's IP address.");
                            sPrinterUUID = string.Empty;
                        }
                    }
                }
                else if (port.sDescription == "Standard TCP/IP Port")
                {
                    try
                    {
                        string ip_address = GetARecord(sPrinterName);
                        Console.WriteLine("IP Address of printer on this port: " + ip_address);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not recover TCP port's IP address.");
                        sPrinterUUID = string.Empty;
                    }
                }

            }
            Console.WriteLine("fini");
        }

        /// <summary>
        /// GetPrinterPorts
        /// Get a list of printer ports on a named entity. 
        /// Throws exception on error
        /// </summary>
        /// <param name="sServer"> Named entity to retrieve list of ports from</param>
        /// <param name="lstPorts"> Collection of port name strings</param>
        public static List<CPrinterPort> GetPrinterPorts(string sServer)
        {
            uint cbNeeded = 0;
            uint cReturned = 0;
            int lastWin32Error = 0;
            IntPtr pAddr = IntPtr.Zero;
            List<CPrinterPort> lstPorts = new List<CPrinterPort>();

            EnumPorts(sServer, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);
            lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error == ERROR_INSUFFICIENT_BUFFER)
            {
                try
                {
                    pAddr = Marshal.AllocHGlobal((int)cbNeeded);
                    if (EnumPorts(sServer, 2, pAddr, cbNeeded, ref cbNeeded, ref cReturned))
                    {
                        long offset = pAddr.ToInt64();
                        Type type = typeof(PORT_INFO_2);
                        int increment = Marshal.SizeOf(type);
                        for (int i = 0; i < cReturned; i++)
                        {
                            PORT_INFO_2 pi2 = (PORT_INFO_2)Marshal.PtrToStructure(new IntPtr(offset), type);
                            CPrinterPort pp = new CPrinterPort();
                            pp.sPortName = pi2.pPortName;
                            pp.sDescription = pi2.pDescription;
                            pp.sMonitorName = pi2.pMonitorName;
                            pp.bPortType = (byte)pi2.fPortType;
                            lstPorts.Add(pp);
                            offset += increment;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("EnumPorts Win32 error: " + lastWin32Error.ToString());
                }
                finally
                {
                    if (pAddr != IntPtr.Zero)
                        Marshal.FreeHGlobal(pAddr);
                }
            }
            else
            {
                throw new Exception("EnumPorts Win32 error, not ERROR_INSUFFICIENT_BUFFER on first allocation attempt.");
            }

            return lstPorts;
        }

        /// <summary>
        /// GetInfoFromPrinterUUID
        /// 
        /// 
        /// </summary>
        /// <param name="sPrinterUUID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string[] GetInfoFromPrinterUUID(string sPrinterUUID)
        {
            string sSubKey = IPP_HIVE_REG + "\\" + sPrinterUUID;
            string[] sPrinterInfo = new string[2];
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
            {
                using (RegistryKey rk = hklm.OpenSubKey(sSubKey))
                {
                    if (rk == null)
                    {
                        throw new Exception("N/A");
                    }
                    try
                    {
                        string temp = rk.GetValue("FriendlyName", "INVALID", RegistryValueOptions.None).ToString().Trim();
                        if (temp == "INVALID")
                        {
                            throw new Exception("Empty Value");
                        }
                        else
                        {
                            sPrinterInfo[0] = temp;
                        }
                        string temp2 = rk.GetValue("DeviceDesc", "INVALID", RegistryValueOptions.None).ToString().Trim();
                        if (temp2 == "INVALID")
                        {
                            throw new Exception("Empty Value");
                        }
                        else
                        {
                            sPrinterInfo[1] = temp2;
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Exception thrown attempting to recover printer uuid from port name: " + ex.Message);
                    }
                }
            }
            return sPrinterInfo;
        }

        public static string GetPrinterUUIDFromPortName(string sPortName)
        {
            string sSubKey = PORT_HIVE_REG + "\\" + sPortName;
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
            {
                using (RegistryKey rk = hklm.OpenSubKey(sSubKey))
                {
                    if (rk == null)
                    {
                        throw new Exception("N/A");
                    }
                    try
                    {
                        string temp = rk.GetValue("Printer UUID", "INVALID", RegistryValueOptions.None).ToString().Trim();
                        if (temp == "INVALID")
                        {
                            throw new Exception("Empty Value");
                        }
                        else
                        {
                            return temp;
                        }

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Exception thrown attempting to recover printer uuid from port name: " + ex.Message);
                    }
                }
            }
        }

        public static string GetARecord(string sHostName)
        {
            IPHostEntry ipEntry;
            IPAddress[] ipAddresses;
            string sRet = string.Empty;

            try
            {
                ipEntry = Dns.GetHostEntry(sHostName);
                ipAddresses = ipEntry.AddressList;
                sRet = ipAddresses[0].ToString();
                return sRet;
            }
            catch (Exception exa)
            {
                return ("Error, " + exa.Message);
            }
        }

        public static string PrintPortType(byte portType)
        {
            StringBuilder spt = new StringBuilder();
            if ((portType & (byte)PortType.Write) == (byte)PortType.Write)
                spt.Append("\nWrite");
            if ((portType & (byte)PortType.Read) == (byte)PortType.Read)
                spt.Append("\nRead");
            if ((portType & (byte)PortType.Redirected) == (byte)PortType.Redirected)
                spt.Append("\nRedirected");
            if ((portType & (byte)PortType.NetAttached) == (byte)PortType.NetAttached)
                spt.Append("\nNet Attached");
            return spt.ToString();
        }

        public class CPrinterPort
        {
            public string sPortName;
            public string sMonitorName;
            public string sDescription;
            public byte bPortType;
        }

    }
}
