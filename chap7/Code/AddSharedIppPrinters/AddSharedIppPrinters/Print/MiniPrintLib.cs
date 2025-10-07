

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace AddSharedIppPrinters.Print
{

    /// <summary>
    /// MiniPrintLib
    /// 
    /// Static methods for adding/removing print connections
    /// </summary>

    public static class MiniPrintLib
    {

        public static int ERROR_SUCCESS = 0;
        public static int ERROR_INSUFFICIENT_BUFFER = 122;
        public static int ERROR_INVALID_NAME = 123;
        public static int ERROR_INVALID_FUNCTION = 1;
        public static int HOSTNAME_MAX_LENGTH = 15;
        public static int SHARED_PRINTER_NAME_LENGTH = HOSTNAME_MAX_LENGTH + 31 + 4;
        private static List<PORT_STRUCT> lstPortStructs = new List<PORT_STRUCT>();
        private static string m_sRegEx = string.Empty;
        private static string sPortMonitorString = string.Empty;
        private static string m_sVersion = string.Empty;
        private static string m_sFileName = string.Empty;



        public enum PrinterInfo
        {
            PRINTER_STATUS = 0,
            PRINTER_ATTRIBUTES = 1
        }

        /*================= Interop Structures==================*/

        //--------- Message Broadcast Consts ----------
        public const int HWND_BROADCAST = 0xffff;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int SMTO_NORMAL = 0x0000;
        public const int SMTO_BLOCK = 0x0001;
        public const int SMTO_ABORTIFHUNG = 0x0002;
        public const int SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;
        //------- End Message Broadcast Consts ----------

        //---------Printer Attribute Values---------
        public const int PRINTER_ATTRIBUTE_QUEUED = 1;
        public const int PRINTER_ATTRIBUTE_DIRECT = 2;                      //Job sent directly to printer, not queued
        public const int PRINTER_ATTRIBUTE_SHARED = 8;                      //Printer is shared
        public const int PRINTER_ATTRIBUTE_NETWORK = 16;                    //Printer is a network print connection
        public const int PRINTER_ATTRIBUTE_HIDDEN = 32;                     //Currently reserved
        public const int PRINTER_ATTRIBUTE_LOCAL = 64;                      //Printer is a local printer
        public const int PRINTER_ATTRIBUTE_KEEPPRINTEDJOBS = 256;           //Printed jobs are not deleted
        public const int PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST = 512;         //Spooled jobs are printed before those who have not finished spooling
        public const int PRINTER_ATTRIBUTE_RAW_ONLY = 4096;                 //Only Raw data type can be spooled
        public const int PRINTER_ATTRIBUTE_PUBLISHED = 8192;                //Printer is published in AD
        public const int PRINTER_ATTRIBUTE_FAX = 16384;                     //Printer can fax
        public const int PRINTER_ATTRIBUTE_TS = 32768;                      //Printer is currently connected through a terminal server
        //--------End Printer Attribute Values-------

        //----------Printer Status Values-----------
        public const int PRINTER_STATUS_USER_INTERVENTION = 1048576;
        public const int PRINTER_STATUS_PENDING_DELETION = 4;
        public const int PRINTER_STATUS_OUTPUT_BIN_FULL = 2048;
        public const int PRINTER_STATUS_SERVER_UNKNOWN = 8388608;
        public const int PRINTER_STATUS_PAPER_PROBLEM = 64;
        public const int PRINTER_STATUS_OUT_OF_MEMORY = 2097152;
        public const int PRINTER_STATUS_NOT_AVAILABLE = 4096;
        public const int PRINTER_STATUS_INITIALIZING = 32768;
        public const int PRINTER_STATUS_MANUAL_FEED = 32;
        public const int PRINTER_STATUS_WARMING_UP = 65536;
        public const int PRINTER_STATUS_PROCESSING = 16384;
        public const int PRINTER_STATUS_POWER_SAVE = 16777216;
        public const int PRINTER_STATUS_TONER_LOW = 131072;
        public const int PRINTER_STATUS_PAPER_OUT = 16;
        public const int PRINTER_STATUS_PAPER_JAM = 8;
        public const int PRINTER_STATUS_PAGE_PUNT = 524288;
        public const int PRINTER_STATUS_IO_ACTIVE = 256;
        public const int PRINTER_STATUS_DOOR_OPEN = 4194304;
        public const int PRINTER_STATUS_PRINTING = 1024;
        public const int PRINTER_STATUS_NO_TONER = 262144;
        public const int PRINTER_STATUS_WAITING = 8192;
        public const int PRINTER_STATUS_OFFLINE = 128;
        public const int PRINTER_STATUS_PAUSED = 1;
        public const int PRINTER_STATUS_ERROR = 2;
        public const int PRINTER_STATUS_BUSY = 512;
        //--------End Printer Status Values---------

        //--------Printer Access Values-----------
        private const int PRINTER_ACCESS_ADMINISTRATOR = 0x4;
        private const int PRINTER_ACCESS_USE = 0x8;
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int PRINTER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | PRINTER_ACCESS_ADMINISTRATOR | PRINTER_ACCESS_USE;
        //--------End Printer Access Values-----------

        //--------Install Print Driver From Driver Package -------
        public const int IPDFP_COPY_ALL_FILES = 0x1;

        //------------DEVMODE consts--------------
        //color
        public const int DMCOLOR_COLOR = 2;
        public const int DMCOLOR_MONOCHROME = 1;
        //duplex
        public const int DMDUP_HORIZONTAL = 3;
        public const int DMDUP_VERTICAL = 2;
        public const int DMDUP_SIMPLEX = 1;
        //collate
        public const int DMCOLLATE_FALSE = 0;
        public const int DMCOLLATE_TRUE = 1;
        //print quality
        public const int DMRES_DRAFT = -1;
        public const int DMRES_LOW = -2;
        public const int DMRES_MEDIUM = -3;
        public const int DMRES_HIGH = -4;
        //orientation
        public const int DMORIENT_PORTRAIT = 1;
        public const int DMORIENT_LANDSCAPE = 2;
        //-------End DEVMODE consts--------------
        //------- Port Constants --------
        public const int MAX_PORTNAME_LEN = 64;
        public const int MAX_NETWORKNAME_LEN = 49;
        public const int MAX_SNMP_COMMUNITY_STR_LEN = 33;
        public const int MAX_QUEUENAME_LEN = 33;
        public const int MAX_IPADDR_STR_LEN = 16;
        public const int RESERVED_BYTE_ARRAY_SIZE = 540;
        //-------End Port Constants --------

        [FlagsAttribute]
        public enum PrinterEnumFlags
        {
            PRINTER_ENUM_DEFAULT = 0x00000001,
            PRINTER_ENUM_LOCAL = 0x00000002,
            PRINTER_ENUM_CONNECTIONS = 0x00000004,
            PRINTER_ENUM_FAVORITE = 0x00000004,
            PRINTER_ENUM_NAME = 0x00000008,
            PRINTER_ENUM_REMOTE = 0x00000010,
            PRINTER_ENUM_SHARED = 0x00000020,
            PRINTER_ENUM_NETWORK = 0x00000040,
            PRINTER_ENUM_EXPAND = 0x00004000,
            PRINTER_ENUM_CONTAINER = 0x00008000,
            PRINTER_ENUM_ICONMASK = 0x00ff0000,
            PRINTER_ENUM_ICON1 = 0x00010000,
            PRINTER_ENUM_ICON2 = 0x00020000,
            PRINTER_ENUM_ICON3 = 0x00040000,
            PRINTER_ENUM_ICON4 = 0x00080000,
            PRINTER_ENUM_ICON5 = 0x00100000,
            PRINTER_ENUM_ICON6 = 0x00200000,
            PRINTER_ENUM_ICON7 = 0x00400000,
            PRINTER_ENUM_ICON8 = 0x00800000,
            PRINTER_ENUM_HIDE = 0x01000000
        }

        [Flags()]
        public enum DM : int
        {
            Orientation = 0x1,
            PaperSize = 0x2,
            PaperLength = 0x4,
            PaperWidth = 0x8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MediaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        public struct POINTL
        {
            public Int32 x;
            public Int32 y;
        }

        /*=================================================================
         * Notice - below, the pointer size is set to 64 bit versus the
         * 32 bit size on WIndows 7 - this is very important!
        =================================================================*/
        public const int CCHDEVICENAME = 64;
        public const int CCHFORMNAME = 64;
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [System.Runtime.InteropServices.FieldOffset(0)]
            public string dmDeviceName;
            [System.Runtime.InteropServices.FieldOffset(64)]
            public Int16 dmSpecVersion;
            [System.Runtime.InteropServices.FieldOffset(66)]
            public Int16 dmDriverVersion;
            [System.Runtime.InteropServices.FieldOffset(68)]
            public Int16 dmSize;
            [System.Runtime.InteropServices.FieldOffset(70)]
            public Int16 dmDriverExtra;
            [System.Runtime.InteropServices.FieldOffset(72)]
            public DM dmFields;
            [System.Runtime.InteropServices.FieldOffset(76)]
            Int16 dmOrientation;
            [System.Runtime.InteropServices.FieldOffset(78)]
            Int16 dmPaperSize;
            [System.Runtime.InteropServices.FieldOffset(80)]
            Int16 dmPaperLength;
            [System.Runtime.InteropServices.FieldOffset(82)]
            Int16 dmPaperWidth;
            [System.Runtime.InteropServices.FieldOffset(84)]
            Int16 dmScale;
            [System.Runtime.InteropServices.FieldOffset(86)]
            Int16 dmCopies;
            [System.Runtime.InteropServices.FieldOffset(88)]
            Int16 dmDefaultSource;
            [System.Runtime.InteropServices.FieldOffset(90)]
            Int16 dmPrintQuality;
            [System.Runtime.InteropServices.FieldOffset(76)]
            public POINTL dmPosition;
            [System.Runtime.InteropServices.FieldOffset(84)]
            public Int32 dmDisplayOrientation;
            [System.Runtime.InteropServices.FieldOffset(88)]
            public Int32 dmDisplayFixedOutput;
            [System.Runtime.InteropServices.FieldOffset(92)]
            public Int16 dmColor;
            [System.Runtime.InteropServices.FieldOffset(94)]
            public short dmDuplex;
            [System.Runtime.InteropServices.FieldOffset(96)]
            public short dmYResolution;
            [System.Runtime.InteropServices.FieldOffset(98)]
            public short dmTTOption;
            [System.Runtime.InteropServices.FieldOffset(100)]
            public short dmCollate;
            [System.Runtime.InteropServices.FieldOffset(104)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [System.Runtime.InteropServices.FieldOffset(166)]
            public Int16 dmLogPixels;
            [System.Runtime.InteropServices.FieldOffset(168)]
            public Int32 dmBitsPerPel;
            [System.Runtime.InteropServices.FieldOffset(172)]
            public Int32 dmPelsWidth;
            [System.Runtime.InteropServices.FieldOffset(176)]
            public Int32 dmPelsHeight;
            [System.Runtime.InteropServices.FieldOffset(180)]
            public Int32 dmDisplayFlags;
            [System.Runtime.InteropServices.FieldOffset(180)]
            public Int32 dmNup;
            [System.Runtime.InteropServices.FieldOffset(184)]
            public Int32 dmDisplayFrequency;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct JOB_INFO_1
        {
            public UInt32 JobId;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pMachineName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pUserName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDocument;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pStatus;
            public UInt32 Status;
            public UInt32 Priority;
            public UInt32 Position;
            public UInt32 TotalPages;
            public UInt32 PagesPrinted;
            public SYSTEMTIME Submitted;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pShareName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPortName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pComment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pLocation;
            public IntPtr pDevMode;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pSepFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pParameters;
            private IntPtr pSecurityDescriptor;
            public uint Attributes;
            public uint Priority;
            public uint DefaultPriority;
            public uint StartTime;
            public uint UntilTime;
            public uint Status;
            public uint cJobs;
            public uint AveragePPM;
        }

        /// <summary>
        /// Print Job Control enumeration
        /// </summary>
        public enum PRINT_JOB_CONTROL
        {
            JOB_CONTROL_PAUSE = 1,
            JOB_CONTROL_RESUME = 2,
            JOB_CONTROL_CANCEL = 3,
            JOB_CONTROL_RESTART = 4,
            JOB_CONTROL_DELETE = 5,
            JOB_CONTROL_RETAIN = 8,
            JOB_CONTROL_RELEASE = 9
        }

        [Flags]
        public enum PortType
        {
            Write = 0x1,
            Read = 0x2,
            Redirected = 0x4,
            NetAttached = 0x8
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_DEFAULTS
        {
            public IntPtr pDatatype;
            public IntPtr pDevMode;
            public int DesiredAccess;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DRIVER_INFO_8
        {
            public uint cVersion;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pEnvironment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDataFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pConfigFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pHelpFile;
            public IntPtr pDependentFiles;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pMonitorName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDefaultDataType;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzPreviousNames;
            public FILETIME ftDriverDate;
            public Int64 lDriverversion;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszMfgName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszOEMUrl;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszHardwareID;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszProvider;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszVendorSetup;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzColorProfiles;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszInfPath;
            public uint dwPrinterDriverAttributes;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzCoreDriverDependencies;
            public FILETIME ftMinInboxDriverVerDate;
            public Int64 lMinInboxDriverVerVersion;
        }

        private struct PORT_STRUCT
        {
            public string m_sPortName;
            public string m_sMonitorName;
        }

        public enum DesiredAccess
        {
            ServerAdmin = 0x01,
            ServerEnum = 0x02,
            PrinterAdmin = 0x04,
            PrinterUse = 0x08,
            JobAdmin = 0x10,
            JobRead = 0x20,
            StandardRightsRequired = 0x000f0000,
            PrinterAllAccess = (StandardRightsRequired | PrinterAdmin | PrinterUse)
        }

        public enum DELETE_FLAG_OPTIONS
        {
            DPD_DELETE_UNUSED_FILES = 0x01,
            DPD_DELETE_SPECIFIC_VERSION = 0x02,
            DPD_DELETE_ALL_FILES = 0x04
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct PortData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PORTNAME_LEN)]
            public string sztPortName;
            public UInt32 dwVersion;
            public UInt32 dwProtocol;
            public UInt32 cbSize;
            public UInt32 dwReserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_NETWORKNAME_LEN)]
            public string sztHostAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_SNMP_COMMUNITY_STR_LEN)]
            public string sztSNMPCommunity;
            public UInt32 dwDoubleSpool;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_QUEUENAME_LEN)]
            public string sztQueue;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_IPADDR_STR_LEN)]
            public string sztIPAddress;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = RESERVED_BYTE_ARRAY_SIZE)]
            public byte[] Reserved;
            public UInt32 dwPortNumber;
            public UInt32 dwSNMPEnabled;
            public UInt32 dwSNMPDevIndex;
        }


        /*============ Interop APIs===============*/
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumPrinters(PrinterEnumFlags Flags, string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnumPorts(string pName, uint level, IntPtr lpbPorts, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int XcvDataW(IntPtr hXcv, [MarshalAs(UnmanagedType.LPWStr)] string pszDataName, IntPtr pInputData, uint cbInputData, IntPtr pOutputData, uint cbOutputData, out uint pcbOutputNeeded, out uint pdwStatus);
        [DllImport("winspool.drv", EntryPoint = "OpenPrinter", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern long OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);
        [DllImport("winspool.drv", EntryPoint = "OpenPrinter", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern long OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr NULL);
        [DllImport("winspool.drv", EntryPoint = "ClosePrinter", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ClosePrinter(IntPtr hPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddPrinterConnection(String pszBuffer);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeletePrinterConnection(String pName);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetDefaultPrinter(String Name);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumPrinterDrivers(String pName, String pEnvironment, uint level, IntPtr pDriverInfo, uint cdBuf, ref uint pcbNeeded, ref uint pcRetruned);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SendMessageTimeout(IntPtr hWnd, int Msg, int wParam, string lParam, int fuFlags, int uTimeout, out int lpdwResult);
        [DllImport("printui.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern void PrintUIEntryW(IntPtr hwnd, IntPtr hinst, string lpszCmdLine, int nCmdShow);
        [DllImport("Winspool.drv", SetLastError = true, EntryPoint = "EnumJobsW")]
        private static extern int EnumJobs(IntPtr hPrinter, uint FirstJob, uint NoJobs, uint Level, IntPtr pJob, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);
        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool GetPrinterDriverDirectory(string pName, string pEnv, int Level, [Out] StringBuilder outPath, int bufferSize, ref int Bytes);
        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool GetPrintProcessorDirectory(string pName, string pEnv, int Level, [Out] StringBuilder outPath, int bufferSize, ref int Bytes);
        [DllImport("winspool.drv", EntryPoint = "SetJobW", SetLastError = true)]
        private static extern bool SetJob(IntPtr hPrinter, int JobId, int Level, IntPtr pJob, int command);
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetPrinterW")]
        private static extern bool GetPrinter(IntPtr hPrinter, uint dwLevel, IntPtr pPrinter, uint dwBuf, ref uint dwNeeded);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool XcvDataW(IntPtr hXcv, string pszDataName, IntPtr pInputData, UInt32 cbInputData, out IntPtr pOutputData, UInt32 cbOutputData, out UInt32 pcbOutputNeeded, out UInt32 pdwStatus);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int AddPrinter(string pName, uint Level, [In] ref PRINTER_INFO_2 pPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern long DeletePrinter(IntPtr hPrinter);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool DeletePrinterDriverW([System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pName, [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pEnvironment, [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pDriverName);
        [DllImport("winspool.drv", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool DeletePrinterDriverExW([System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pName, [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string sEnvironment, [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pDriverName, uint dwDeleteFlag, uint dwVersionFlag);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern Int32 InstallPrinterDriverFromPackage([MarshalAs(UnmanagedType.LPTStr)] string sServer, [MarshalAs(UnmanagedType.LPTStr)] string sInfPath, [MarshalAs(UnmanagedType.LPTStr)] string sDriverName, [MarshalAs(UnmanagedType.LPTStr)] string sEnvironment, Int32 Flags);

        /*============ End Interop APIs===============*/
        /// <summary>
        /// GetDefaultPrinterWrapper
        /// </summary>
        /// <param name="sb">Allocated StringBuilder to size of iBufferSize</param>
        /// <param name="iBufferSize">Size of allocated StringBuilder</param>
        /// <returns></returns>
        public static void GetDefaultPrinterWrapper(ref StringBuilder sb, ref int iBufferSize)
        {
            int lastWin32Error = 0;
            try
            {
                if (GetDefaultPrinter(sb, ref iBufferSize) == false)
                {
                    lastWin32Error = Marshal.GetLastWin32Error();
                }
            }
            catch (Exception egdp)
            {
                throw new Exception(egdp.Message);
            }
        }

        /// <summary>
        /// SetAsDefault
        /// Sets the default printer 
        /// </summary>
        /// <param name="sPrinter">in format \\server\share</param>
        /// <returns></returns>
        public static void SetAsDefault(string sPrinter)
        {
            int lastWin32Error = 0;
            try
            {
                if (SetDefaultPrinter(sPrinter) == 0)
                {
                    lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Exception("SetDefaultPrinter API failed, Win32 error: " + lastWin32Error);
                }
            }
            catch (Exception edp)
            {
                throw new Exception(edp.Message);
            }

        }


        /// <summary>
        /// ServerEnumPrinters
        /// Does a remote enumeration of all printers on a given server. 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="printer_list"></param>
        /// <returns></returns>
        public static void ServerEnumPrinters(string server, ref PrinterConnections _pcs)
        {
            uint cbNeeded = 0;
            uint cReturned = 0;
            int lastWin32Error = 0;
            string sHostAddress = string.Empty;
            string sMonitorName = string.Empty;
            string s_fqdn = GetDomainInfo(server);
            IntPtr pAddr = IntPtr.Zero;

            if ((server == null) || (server.Length == 0))
            {
                throw new Exception("Server argument cannot be empty!");
            }

            //clear the collection
           _pcs.Clear();

            try
            {
                EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_NAME | PrinterEnumFlags.PRINTER_ENUM_SHARED, "\\\\" + server, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);
                lastWin32Error = Marshal.GetLastWin32Error();

                if (lastWin32Error == ERROR_INSUFFICIENT_BUFFER)
                {
                    pAddr = Marshal.AllocHGlobal((int)cbNeeded);
                    if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_NAME | PrinterEnumFlags.PRINTER_ENUM_SHARED, "\\\\" + server, 2, pAddr, cbNeeded, ref cbNeeded, ref cReturned))
                    {
                        PRINTER_INFO_2[] pi2 = new PRINTER_INFO_2[cReturned];
                        long offset = pAddr.ToInt64();
                        Type type = typeof(PRINTER_INFO_2);
                        int increment = Marshal.SizeOf(type);
                        for (int i = 0; i < cReturned; i++)
                        {
                            try
                            {
                                pi2[i] = (PRINTER_INFO_2)Marshal.PtrToStructure(new IntPtr(offset), type);
                                // Below does not work for WSD Ports, only TCP ports
                                sHostAddress = GetHostAddress(pi2[i].pServerName, pi2[i].pPortName);
                                PrinterConnection printer = new PrinterConnection();
                                printer.PRINTERNAME = pi2[i].pPrinterName;


                                //If possible, put the server's domain string in here so we know what domain the printers are from...
                                if (!string.IsNullOrEmpty(s_fqdn))
                                {
                                    printer.DOMAIN_PREFIX = s_fqdn;
                                }

                                GetSomePrinterAttributes(printer, pi2[i].Attributes);

                                printer.PRINTERNAME = pi2[i].pPrinterName;
                                //printer.PORTMONITORNAME = sMonitorName;
                                printer.DRIVERNAME = pi2[i].pDriverName;
                                printer.NET_ADDRESS = sHostAddress;
                                printer.DATA_TYPE = pi2[i].pDatatype;
                                printer.ATTRIBUTES = pi2[i].Attributes;
                                printer.COMMENT = pi2[i].pComment;
                                printer.LOCATION = pi2[i].pLocation;
                                printer.PARAMETERS = pi2[i].pParameters;
                                printer.PORTNAME = pi2[i].pPortName;
                                printer.PRINT_PROCESSOR = pi2[i].pPrintProcessor;
                                printer.PRIORITY = pi2[i].Priority;
                                printer.SEPERATOR_FILE = pi2[i].pSepFile;
                                printer.STATUS = GetPrinterStatusString(printer, pi2[i].Status);
                                printer.SERVERNAME = pi2[i].pServerName;
                                printer.SHARENAME = pi2[i].pShareName;
                                printer.AveragePPM =  pi2[i].AveragePPM;
                                printer.IsPrinterReady = IsPrinterReady(pi2[i].Status);

                                //Get the values from the initial device mode
                                GetDevModeValues(printer, pi2[i].pDevMode);

                                _pcs.AddIppPrinterConnection(printer);
                            }
                            catch (Exception ee)
                            {
                                throw new Exception(string.Format("Error enumerating printer: {0}, exception message {1}", pi2[i].pPrinterName, ee.Message));
                            }
                            //Increment pointer...
                            offset += increment;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                lastWin32Error = Marshal.GetLastWin32Error();
                throw new Exception(string.Format("EnumPrinters Exception, Win32 Error: {0}, exception message: {1}", lastWin32Error.ToString(), ex.Message));
            }
            finally
            {
                if (pAddr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pAddr);
                }
            }
        }

        /// <summary>
        /// LocalEnumPrinters
        /// 
        /// Lightweight enumeration of local printers, returns a collection of printer strings
        /// Set the flags argument to dictate what types of printers are returned
        /// </summary>
        /// <param name="flags"> Enumeration of PrinterFlags (See PrinterFlags Enumeration)
        /// PrinterEnumFlags.PRINTER_ENUM_NAME | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS | PrinterEnumFlags.PRINTER_ENUM_LOCAL</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<string> LocalEnumPrinters(PrinterEnumFlags flags)
        {
            uint cbNeeded = 0;
            uint cReturned = 0;
            int lastWin32Error = 0;
            List<string> printers = new List<string>();
            IntPtr pAddr = IntPtr.Zero;

            try
            {
                EnumPrinters(flags, null, 2, IntPtr.Zero, 0, ref cbNeeded, ref cReturned);

                //If there are no local printers matching the flags, then we get back a 0.
                
                if(cbNeeded == 0) 
                {
                    return printers;
                }

                lastWin32Error = Marshal.GetLastWin32Error();

                if (lastWin32Error == ERROR_INSUFFICIENT_BUFFER)
                {
                    pAddr = Marshal.AllocHGlobal((int)cbNeeded);
                    if (EnumPrinters(flags, null, 2, pAddr, cbNeeded, ref cbNeeded, ref cReturned))
                    {
                        long offset = pAddr.ToInt64();
                        Type type = typeof(PRINTER_INFO_2);
                        int increment = Marshal.SizeOf(type);
                        for (int i = 0; i < cReturned; i++)
                        {
                            PRINTER_INFO_2 pi2 = (PRINTER_INFO_2)Marshal.PtrToStructure(new IntPtr(offset), type);
                            printers.Add(pi2.pPrinterName);
                            offset += increment;
                        }
                    }
                }
                else
                {
                    throw new Exception("EnumPrinters Exception, Win32 Error: " + lastWin32Error.ToString());
                }
            }
            catch(Exception ex) 
            {
                lastWin32Error = Marshal.GetLastWin32Error();
                throw new Exception("EnumPrinters error, reason  " + ex.Message);
            }
            finally
            {
                if (pAddr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pAddr);
                }
            }

            return printers;
        }


        /// <summary>
        /// GetDevModeValues
        /// </summary>
        /// <param name="pDevMode"></param>
        public static void GetDevModeValues(PrinterConnection p, IntPtr pDevMode)
        {
            DEVMODE devMode = (DEVMODE)Marshal.PtrToStructure(pDevMode, typeof(DEVMODE));
            devMode.dmSize = (short)Marshal.SizeOf(devMode);

            if (IsFlagSet(devMode.dmColor, DMCOLOR_COLOR) == true)
                p.COLOR_SUPPORT = true;
            else
                p.COLOR_SUPPORT = false;
           
            if ((IsFlagSet(devMode.dmDuplex, DMDUP_HORIZONTAL) == true) || (IsFlagSet(devMode.dmDuplex, DMDUP_VERTICAL) == true))
                p.DUPLEX_SUPPORT = true;
            else
                p.DUPLEX_SUPPORT = false;
            
            if (IsFlagSet(devMode.dmCollate, DMCOLLATE_TRUE) == true)
                p.COLLATE_SUPPORT = true;
            else
                p.COLLATE_SUPPORT = false;
        }

        /// <summary>
        /// AddPrinterConnectionWrapper
        /// </summary>
        /// <param name="sServerPrintShare"></param>
        /// <returns></returns>
        public static void AddPrinterConnectionWrapper(string sServerPrintShare)
        {
            try
            {
                if(AddPrinterConnection(sServerPrintShare) == false)
                {
                    throw new Exception("AddPrinterConnection API failed, Win32 error: " + Marshal.GetLastWin32Error());
                }
            }
            catch(Exception e) 
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// DeletePrinterConnectionWrapper
        /// </summary>
        /// <param name="sServerPrintShare"></param>
        /// <returns></returns>
        public static void DeletePrinterConnectionWrapper(string sServerPrintShare)
        {
            try
            {
                if (DeletePrinterConnection(sServerPrintShare) == false)
                {
                    throw new Exception("DeletePrinterConnection API failed, Win32 error: " + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// GetSomePrinterAttributes
        /// </summary>
        /// <param name="pe"></param>
        /// <param name="uiAttributes"></param>
        public static void GetSomePrinterAttributes(PrinterConnection p, uint uiAttributes)
        {

            if ((uiAttributes & PRINTER_ATTRIBUTE_DIRECT) == PRINTER_ATTRIBUTE_DIRECT)
                p.DIRECT_NOT_SPOOLED = true;
            else
                p.DIRECT_NOT_SPOOLED = false;

            if ((uiAttributes & PRINTER_ATTRIBUTE_SHARED) == PRINTER_ATTRIBUTE_SHARED)
                p.IS_SHARED = true;
            else
                p.IS_SHARED = false;

            if ((uiAttributes & PRINTER_ATTRIBUTE_PUBLISHED) == PRINTER_ATTRIBUTE_PUBLISHED)
                p.IS_PUBLISHED = true;
            else
                p.IS_PUBLISHED = false;

            if ((uiAttributes & PRINTER_ATTRIBUTE_FAX) == PRINTER_ATTRIBUTE_FAX)
                p.IS_FAX = true;
            else
                p.IS_FAX = false;

            if ((uiAttributes & PRINTER_ATTRIBUTE_KEEPPRINTEDJOBS) == PRINTER_ATTRIBUTE_KEEPPRINTEDJOBS)
                p.KEEPS_PRINTED_JOBS = true;
            else
                p.KEEPS_PRINTED_JOBS = false;
        }

        /// <summary>
        /// GetHostAddress
        /// Gets the IP address or net name of the port the TCP/IP attached printer
        /// is configured on.
        /// </summary>
        /// <param name="server">Specify server name or leave blank for local machine</param>
        /// <param name="portName">Name of the TCP/IP Port</param>
        /// <returns></returns>
        private static string GetHostAddress(string server, string portName)
        {
            IntPtr pHandle = IntPtr.Zero;
            string pPrinterName = string.Format(server + @"\,XcvPort " + portName, new object[0]);
            if (OpenPrinter(pPrinterName, out pHandle, IntPtr.Zero) == 0)
            {
                return "";
            }
            string sAddress = "";
            if (pHandle != IntPtr.Zero)
            {
                uint uiSizeNeeded = 0;
                uint uiStatus = 0;
                string pszDataName = "IPAddress";
                IntPtr pOutputData = Marshal.AllocHGlobal(0x100);
                if (XcvDataW(pHandle, pszDataName, IntPtr.Zero, 0, pOutputData, 0x100, out uiSizeNeeded, out uiStatus) > 0)
                {
                    sAddress = Marshal.PtrToStringAuto(pOutputData);
                    if (string.IsNullOrEmpty(sAddress))
                    {
                        pszDataName = "HostAddress";
                        if (XcvDataW(pHandle, pszDataName, IntPtr.Zero, 0, pOutputData, 0x100, out uiSizeNeeded, out uiStatus) > 0)
                        {
                            sAddress = Marshal.PtrToStringAuto(pOutputData);
                        }
                    }
                }
                else
                {
                    return "";
                }
                Marshal.FreeHGlobal(pOutputData);
            }
            ClosePrinter(pHandle);

            return sAddress;
        }


        /// <summary>
        /// IsPrinterReady
        /// 
        /// Returns if a printer is ready to print (or almost ready). 
        /// If a printer has a paper jam or is offline, is out of toner, etc. this will
        /// return false
        /// </summary>
        /// <param name="uiStatus"></param>
        /// <returns></returns>
        public static bool IsPrinterReady(uint uiStatus)
        {
            if ((uiStatus & PRINTER_STATUS_BUSY) == PRINTER_STATUS_BUSY)
                return true;
            else if ((uiStatus & PRINTER_STATUS_INITIALIZING) == PRINTER_STATUS_INITIALIZING)
                return true;
            else if ((uiStatus & PRINTER_STATUS_IO_ACTIVE) == PRINTER_STATUS_IO_ACTIVE)
                return true;
            else if ((uiStatus & PRINTER_STATUS_POWER_SAVE) == PRINTER_STATUS_POWER_SAVE)
                return true;
            else if ((uiStatus & PRINTER_STATUS_PRINTING) == PRINTER_STATUS_PRINTING)
                return true;
            else if ((uiStatus & PRINTER_STATUS_PROCESSING) == PRINTER_STATUS_PROCESSING)
                return true;
            else if ((uiStatus & PRINTER_STATUS_WAITING) == PRINTER_STATUS_WAITING)
                return true;
            else if ((uiStatus & PRINTER_STATUS_WARMING_UP) == PRINTER_STATUS_WARMING_UP)
                return true;
            else
                return false;
        }

        /// <summary>
        /// GetPrinterStatusString
        /// </summary>
        /// <param name="uiStatus"></param>
        /// <returns></returns>
        public static string GetPrinterStatusString(PrinterConnection p, uint uiStatus)
        {
            string sError = string.Empty;
            p.ASSISTANCE_NEEDED = true;

            if ((uiStatus & PRINTER_STATUS_BUSY) == PRINTER_STATUS_BUSY)
                sError = "The printer is busy";
            else if ((uiStatus & PRINTER_STATUS_DOOR_OPEN) == PRINTER_STATUS_DOOR_OPEN)
                sError = "The printer door is open";
            else if ((uiStatus & PRINTER_STATUS_ERROR) == PRINTER_STATUS_ERROR)
                sError = "The printer is in an error state.";
            else if ((uiStatus & PRINTER_STATUS_INITIALIZING) == PRINTER_STATUS_INITIALIZING)
                sError = "The printer is initializing.";
            else if ((uiStatus & PRINTER_STATUS_IO_ACTIVE) == PRINTER_STATUS_IO_ACTIVE)
                sError = "The printer is in an active input/output state.";
            else if ((uiStatus & PRINTER_STATUS_MANUAL_FEED) == PRINTER_STATUS_MANUAL_FEED)
                sError = "The printer is in a manual feed state.";
            else if ((uiStatus & PRINTER_STATUS_NO_TONER) == PRINTER_STATUS_NO_TONER)
                sError = "The printer is out of toner.";
            else if ((uiStatus & PRINTER_STATUS_NOT_AVAILABLE) == PRINTER_STATUS_NOT_AVAILABLE)
                sError = "The printer is not available for printing.";
            else if ((uiStatus & PRINTER_STATUS_OFFLINE) == PRINTER_STATUS_OFFLINE)
                sError = "The printer is offline.";
            else if ((uiStatus & PRINTER_STATUS_OUT_OF_MEMORY) == PRINTER_STATUS_OUT_OF_MEMORY)
                sError = "The printer has run out of memory.";
            else if ((uiStatus & PRINTER_STATUS_OUTPUT_BIN_FULL) == PRINTER_STATUS_OUTPUT_BIN_FULL)
                sError = "The printer's output bin is full.";
            else if ((uiStatus & PRINTER_STATUS_PAGE_PUNT) == PRINTER_STATUS_PAGE_PUNT)
                sError = "The printer cannot print the current page.";
            else if ((uiStatus & PRINTER_STATUS_PAPER_JAM) == PRINTER_STATUS_PAPER_JAM)
                sError = "Paper is jammed in the printer";
            else if ((uiStatus & PRINTER_STATUS_PAPER_OUT) == PRINTER_STATUS_PAPER_OUT)
                sError = "The printer is out of paper.";
            else if ((uiStatus & PRINTER_STATUS_PAPER_PROBLEM) == PRINTER_STATUS_PAPER_PROBLEM)
                sError = "The printer has a paper problem.";
            else if ((uiStatus & PRINTER_STATUS_PAUSED) == PRINTER_STATUS_PAUSED)
                sError = "The printer is paused.";
            else if ((uiStatus & PRINTER_STATUS_PENDING_DELETION) == PRINTER_STATUS_PENDING_DELETION)
                sError = "The printer is being deleted.";
            else if ((uiStatus & PRINTER_STATUS_POWER_SAVE) == PRINTER_STATUS_POWER_SAVE)
                sError = "The printer is in power save mode.";
            else if ((uiStatus & PRINTER_STATUS_PRINTING) == PRINTER_STATUS_PRINTING)
                sError = "The printer is printing.";
            else if ((uiStatus & PRINTER_STATUS_PROCESSING) == PRINTER_STATUS_PROCESSING)
                sError = "The printer is processing a print job.";
            else if ((uiStatus & PRINTER_STATUS_SERVER_UNKNOWN) == PRINTER_STATUS_SERVER_UNKNOWN)
                sError = "The printer status is unknown.";
            else if ((uiStatus & PRINTER_STATUS_TONER_LOW) == PRINTER_STATUS_TONER_LOW)
                sError = "The printer is low on toner.";
            else if ((uiStatus & PRINTER_STATUS_USER_INTERVENTION) == PRINTER_STATUS_USER_INTERVENTION)
                sError = "User intervention required.";
            else if ((uiStatus & PRINTER_STATUS_WAITING) == PRINTER_STATUS_WAITING)
                sError = "The printer is waiting.";
            else if ((uiStatus & PRINTER_STATUS_WARMING_UP) == PRINTER_STATUS_WARMING_UP)
                sError = "The printer is warming up.";
            else
            {
                p.ASSISTANCE_NEEDED = false;
                sError = "Printer is OK";
            }

            return sError;
        }

        /// <summary>
        /// IsFlagSet
        /// Does a binary check on a bit field to see if the given flag is set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private static bool IsFlagSet<T>(T value, T flags)
        {
            return ((Convert.ToInt32(value) & Convert.ToInt32(flags)) == Convert.ToInt32(flags));
        }


        /// <summary>
        /// FiletimeToDateTime
        /// Converts Win32 FILETIME structure into a managed code DateTime object
        /// </summary>
        /// <param name="fileTime"></param>
        /// <returns></returns>
        private static DateTime FiletimeToDateTime(FILETIME fileTime)
        {
            long hFT2 = (((long)fileTime.DateTimeHigh) << 32) | ((uint)fileTime.DateTimeLow);
            return DateTime.FromFileTimeUtc(hFT2);
        }

        /// <summary>
        /// SystemTimeToDateTime
        /// Converts unmanaged SystemTime to managed code's DateTime structure
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        private static DateTime SystemTimeToDateTime(SYSTEMTIME st)
        {
            DateTime dt = new DateTime(st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second, st.Milliseconds, DateTimeKind.Local);
            return dt;
        }


        public static string GetDomainInfo(string sHostName)
        {
            IPHostEntry ipEntry;
            string sRet = string.Empty;
            int pos = 0;

            try
            {
                pos = sHostName.IndexOf("\\\\");
                if (pos != -1)
                {
                    sHostName = sHostName.Substring(2);
                }
                ipEntry = Dns.GetHostEntry(sHostName);
                string fqdn = ipEntry.HostName;
                pos = fqdn.IndexOf(".");
                if (pos == -1)
                    return "";
                else
                {
                    pos++;
                    return fqdn.Substring(pos);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}
