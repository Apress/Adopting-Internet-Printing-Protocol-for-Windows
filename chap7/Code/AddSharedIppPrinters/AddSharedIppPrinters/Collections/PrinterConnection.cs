using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace AddSharedIppPrinters
{
    /// <summary>
    /// Printer
    /// 
    /// Class that describes basic print attributes used by this program.
    /// </summary>
    public class PrinterConnection : IComparable
    {
        
        //Attributes
        public const int ATTRIBUTE_COLOR = 1;
        public const int ATTRIBUTE_DUPLEX = 2;
        public const int ATTRIBUTE_COLLATE = 4;
        public const int ATTRIBUTE_FAX = 8;
        public const int ATTRIBUTE_PUBLISHED = 16;
        

        private string sPrinterName = string.Empty;
        private bool m_bLocallyInstalled = false;
        private bool _isDefault = false;
        private bool _isServerBased = false;
        private string m_sLocation = string.Empty;
        private bool m_bAssistanceNeeded = false;
        private string m_sPrinterFriendlyName = string.Empty;
        private string m_sDriverName = string.Empty;
        private string m_sDomainPrefix = string.Empty;
        private bool _isPrinterReady = false;
        //
        private string m_sServerName = string.Empty;
        private string m_sQueueName = string.Empty;
        private string m_sShareName = string.Empty;
        private string m_sPortName = string.Empty;
        private string m_sNetAddress = string.Empty;
        private string m_sComment = string.Empty;
        private string m_sSepFile = string.Empty;
        private string m_sPrintProcessor = string.Empty;
        private string m_sDatatype = string.Empty;
        private string m_sParameters = string.Empty;
        private string m_sPortMonitorName = string.Empty;
        private uint m_uiAttributes = 0;
        private uint m_uiPriority = 0;
        private uint m_uiAveragePPM = 0;
        private string m_sPrinterName = string.Empty;
        private string m_sStatus = string.Empty;
        //DevMode values
        private bool m_bColorSupport = false;
        private bool m_bDuplexSupport = false;
        private bool m_bCollateSupport = false;
        //Boolean Attributes
        private bool m_bDirect = false;
        private bool m_bKeepPrintedJobs = false;
        private bool m_bPublished = false;
        private bool m_bShared = false;
        private bool m_bFax = false;
        private bool m_bWSDPort = false;
        //

      
        public PrinterConnection()
        {
          
        }

        public string DRIVERNAME
        {
            get { return m_sDriverName; }
            set { m_sDriverName = value; }
        }
       
        public string PRINTERNAME
        {
            set { sPrinterName = value;}
            get { return sPrinterName; }
        }

        public string LOCATION
        {
            set { m_sLocation = value;}
            get { return m_sLocation; }
        }

        public string DOMAIN_PREFIX
        {
            get { return m_sDomainPrefix; }
            set { m_sDomainPrefix = value; }
        }

        public bool ASSISTANCE_NEEDED
        {
            set { m_bAssistanceNeeded = value; }
            get { return m_bAssistanceNeeded; }
        }

        /// <summary>
        /// Driver is locally installed
        /// </summary>
        public bool LOCALLY_INSTALLED
        {
            set { m_bLocallyInstalled = value; }
            get { return m_bLocallyInstalled; }
        }

        /// Is this the default printer?
        /// </summary>
        public bool IS_DEFAULT
        {
            set 
            { 
                _isDefault = value;
            }
            get 
            { 
                return _isDefault; 
            }
        }

        /// <summary>
        /// Is this server based?
        /// </summary>
        public bool IS_SERVER_BASED
        {
            set { _isServerBased = value; }
            get { return _isServerBased; }
        }

        public bool IsPrinterReady 
        { 
            get => _isPrinterReady; 
            set => _isPrinterReady = value; 
        }

        /// <summary>
        /// HasAttributes
        /// Does this printer have the attributes requested?
        /// </summary>
        /// <param name="iAttributes"></param>
        /// <param name="bOr">TRUE for OR operation, FALSE for AND operation on attributes</param>
        /// <returns></returns>
        public bool HasAttributes(int iAttributes, bool bOr)
        {

            if (bOr == false)
            {
                //Is an AND operation
                if (((iAttributes & ATTRIBUTE_COLOR) == ATTRIBUTE_COLOR) && (COLOR_SUPPORT == false))
                {
                    return false;
                }
                if (((iAttributes & ATTRIBUTE_DUPLEX) == ATTRIBUTE_DUPLEX) && (DUPLEX_SUPPORT == false))
                {
                    return false;
                }
                if (((iAttributes & ATTRIBUTE_COLLATE) == ATTRIBUTE_COLLATE) && (COLLATE_SUPPORT == false))
                {
                    return false;
                }
                if (((iAttributes & ATTRIBUTE_FAX) == ATTRIBUTE_FAX) && (IS_FAX == false))
                {
                    return false;
                }
                if (((iAttributes & ATTRIBUTE_PUBLISHED) == ATTRIBUTE_PUBLISHED) && (IS_PUBLISHED == false))
                {
                    return false;
                }
                return true;
            }
            else
            {
                // Is an Or operation
                if (((iAttributes & ATTRIBUTE_COLOR) == ATTRIBUTE_COLOR) && (COLOR_SUPPORT == true))
                {
                    return true;
                }
                if (((iAttributes & ATTRIBUTE_DUPLEX) == ATTRIBUTE_DUPLEX) && (DUPLEX_SUPPORT == true))
                {
                    return true;
                }
                if (((iAttributes & ATTRIBUTE_COLLATE) == ATTRIBUTE_COLLATE) && (COLLATE_SUPPORT == true))
                {
                    return true;
                }
                if (((iAttributes & ATTRIBUTE_FAX) == ATTRIBUTE_FAX) && (IS_FAX == true))
                {
                    return true;
                }
                if (((iAttributes & ATTRIBUTE_PUBLISHED) == ATTRIBUTE_PUBLISHED) && (IS_PUBLISHED == true))
                {
                    return true;
                }
                return false;
            }
        }
        //
        public string FRIENDLY_PRINTER_NAME
        {
            set { m_sPrinterFriendlyName = value; }
            get { return m_sPrinterFriendlyName; }
        }

        public string SERVERNAME
        {
            set { m_sServerName = value; }
            get { return m_sServerName; }
        }

        public string STATUS
        {
            set { m_sStatus = value; }
            get { return m_sStatus; }
        }

        public string QUEUE_NAME
        {
            set { m_sQueueName = value; }
            get { return m_sQueueName; }
        }
        public string SHARENAME
        {
            set { m_sShareName = value; }
            get { return m_sShareName; }
        }
        public string PORTNAME
        {
            set 
            { 
                m_sPortName = value; 
                if(m_sPortName.StartsWith("WSD"))
                {
                    IS_WSD_PORT = true;
                }
                else
                {
                    IS_WSD_PORT = false;
                }
            }
            get { return m_sPortName; }
        }
        public string NET_ADDRESS
        {
            set { m_sNetAddress = value; }
            get { return m_sNetAddress; }
        }

        public bool IS_WSD_PORT
        {
            get { return m_bWSDPort; }
            set { m_bWSDPort = value; }
        }
        public string COMMENT
        {
            set { m_sComment = value; }
            get { return m_sComment; }
        }

        public string SEPERATOR_FILE
        {
            set { m_sSepFile = value; }
            get { return m_sSepFile; }
        }
        public string PRINT_PROCESSOR
        {
            set { m_sPrintProcessor = value; }
            get { return m_sPrintProcessor; }
        }
        public string DATA_TYPE
        {
            set { m_sDatatype = value; }
            get { return m_sDatatype; }
        }
        public string PARAMETERS
        {
            set { m_sParameters = value; }
            get { return m_sParameters; }
        }

        public uint ATTRIBUTES
        {
            set { m_uiAttributes = value; }
            get { return m_uiAttributes; }
        }

        public uint PRIORITY
        {
            set { m_uiPriority = value; }
            get { return m_uiPriority; }
        }

        public bool COLOR_SUPPORT
        {
            set { m_bColorSupport = value; }
            get { return m_bColorSupport; }
        }

        public bool DUPLEX_SUPPORT
        {
            set { m_bDuplexSupport = value; }
            get { return m_bDuplexSupport; }
        }

        public bool COLLATE_SUPPORT
        {
            set { m_bCollateSupport = value; }
            get { return m_bCollateSupport; }
        }

        // Jobs is sent directly to printer (not spooled)
        public bool DIRECT_NOT_SPOOLED
        {
            set
            {
                m_bDirect = value;
            }
            get { return m_bDirect; }
        }

        //Published in AD
        public bool IS_PUBLISHED
        {
            set
            {
                m_bPublished = value;
            }
            get { return m_bPublished; }
        }
      
        //Jobs not deleted from spooler
        public bool KEEPS_PRINTED_JOBS
        {
            set
            {
                m_bKeepPrintedJobs = value;
            }
            get { return m_bKeepPrintedJobs; }
        }
       
        //Printer is Shared
        public bool IS_SHARED
        {
            set
            { m_bShared = value; }
            get { return m_bShared; }
        }
        //Printer can fax
        public bool IS_FAX
        {
            set
            {
                m_bFax = value;
            }
            get { return m_bFax; }
        }

        public uint AveragePPM
        {
            get => m_uiAveragePPM;
            set => m_uiAveragePPM = value;
        }


        ///<remarks>
        /// Required interface for IComparable
        /// Implements CompareTo method for object 
        /// Required for sorting (in this case insensitive by name)
        ///</remarks>
        public int CompareTo(object other)
        {
            return string.Compare(this.sPrinterName, ((PrinterConnection)other).sPrinterName, true);
        }
        

        /// <summary>
        /// ToString OverRide - Used primarily for 508 compliance
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IS_DEFAULT == true)
                return PRINTERNAME + "Is the default printer";
            else
                return PRINTERNAME;
        }
    }
}
