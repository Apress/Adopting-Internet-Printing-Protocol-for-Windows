using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace CS_GPA_CH3
{
    public static class WinIppPinvokeMethods
    {
        //--------------------------PInvokes------------------------------------

        [DllImport("WinIpp.dll", EntryPoint="WrIdentifyPrinter", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int IdentifyPrinter([MarshalAs(UnmanagedType.LPStr)]StringBuilder pHostname, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "freePrinterAttributesArrayMemory", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreePrinterAttributesArrayMemory();

        [DllImport("WinIpp.dll", EntryPoint = "freePrinterJobsMemory", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void freePrinterJobsMemory();

        [DllImport("WinIpp.dll", EntryPoint = "WrGetIppErrorString", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetIppErrorString(int status);

        [DllImport("WinIpp.dll", EntryPoint = "WrCreateJob", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateJob([MarshalAs(UnmanagedType.LPStr)] StringBuilder pHostname, [MarshalAs(UnmanagedType.LPStr)] StringBuilder documentPath, [MarshalAs(UnmanagedType.LPStr)] StringBuilder userName, ref int jobId,string[] jobAttributes, int numAttributes, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "WrSendDocument", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendDocument([MarshalAs(UnmanagedType.LPStr)] StringBuilder pHostname, [MarshalAs(UnmanagedType.LPStr)] StringBuilder documentPath, [MarshalAs(UnmanagedType.LPStr)] StringBuilder userName, int jobId, string[] jobAttributes, int numAttributes, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint) Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "WrPrintJob", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int PrintJob([MarshalAs(UnmanagedType.LPStr)] StringBuilder pHostname, [MarshalAs(UnmanagedType.LPStr)] StringBuilder documentPath, [MarshalAs(UnmanagedType.LPStr)] StringBuilder userName, ref int jobId, string[] jobAttributes, int numAttributes, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "WrCancelJob", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CancelJob([MarshalAs(UnmanagedType.LPStr)] StringBuilder pHostname, [MarshalAs(UnmanagedType.LPStr)] StringBuilder userName, ref int jobId, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "WrGetPrinterAttributes", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetPrinterAttributes([MarshalAs(UnmanagedType.LPStr)] StringBuilder hostname, [MarshalAs(UnmanagedType.LPStr)] StringBuilder userName, ref int num, ref int retCode, uint encryption, uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "WrGetJobs", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetJobs([MarshalAs(UnmanagedType.LPStr)] StringBuilder pHostname, ref int numJobs, uint encryption,uint timeout = 3000, uint ip_family = Defines.AF_UNSPEC, uint chunking = (uint)Defines.TRANSFER_METHOD.IPPTOOL_TRANSFER_CHUNKED);

        [DllImport("WinIpp.dll", EntryPoint = "setCallback", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCallback(IntPtr callback);

        [DllImport("WinIpp.dll")]
        public static extern void InvokeTestCallback(int parameter);

        //--------------------------PInvokes------------------------------------

     

        //---------------------------Methods------------------------------------
        /// <summary>
        /// _identifyPrinter
        /// 
        /// When called, this will either flash the user panel or make a sound depending
        /// on how the printer is configured.
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static int _identifyPrinter(string hostname)
        {
            StringBuilder sb = new StringBuilder(hostname);
            int ret = IdentifyPrinter(sb, (uint)Defines.HTTP_ENCRYPTION.HTTP_ENCRYPTION_ALWAYS);
            return ret;
        }

        /// <summary>
        /// _getprinterAttributes
        /// 
        /// Gets printer attribute pointer from unmanaged code and converts this
        /// into list of strings which are then passed to the caller. 
        /// 
        /// Must call FreePrinterAttributesArrayMemory to free unmanaged memory.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static List<string> _getPrinterAttributes(string hostname, string user_name, uint encryption)
        {
            int size = 0;
            int retCode = 0;
            StringBuilder sb = new StringBuilder(hostname);
            StringBuilder sbun = new StringBuilder(user_name);
            List<string> stringList;

            try
            {
                IntPtr ptr = GetPrinterAttributes(sb, sbun, ref size, ref retCode, (uint) encryption);
                if (retCode == Defines.IPP_STATUS_OK)
                {
                    stringList = new List<string>();
                    for (int i = 0; i < size; i++)
                    {
                        IntPtr strPtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                        stringList.Add(Marshal.PtrToStringAnsi(strPtr));
                    }

                }
                else
                {
                    throw new Exception($"Get Printer Attributes request on {hostname} failed, IPP return code was {retCode}");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                //cleanup
                FreePrinterAttributesArrayMemory();
            }

            return stringList;
        }
    }   
}
