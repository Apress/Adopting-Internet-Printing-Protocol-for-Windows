using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrinterInfo
{
    /// <summary>
    /// CPrinterInfo
    /// 
    /// IP address and community string of printer
    /// </summary>
    public class CPrinterInfo
    {
        private string m_sIpAddress = string.Empty;
        private string m_sCommunityString = string.Empty;
        public const int npos = -1;

        public CPrinterInfo(string s)
        {
            Init(s);
        }

        private void Init(string s)
        {
            int iPos = s.IndexOf(',');
            if (iPos == npos)
            {
                throw new Exception("Invalid printer entry in Printers.txt file: " + s);
            }
            else
            {
                IP_ADDRESS_STRING = s.Substring(0, iPos).Trim();
                COMMUNITY_STRING = s.Substring(iPos + 1).Trim();
            }
        }

        public string IP_ADDRESS_STRING
        {
            get { return m_sIpAddress; }
            set { m_sIpAddress = value; }
        }

        
        public string COMMUNITY_STRING
        {
            get { return m_sCommunityString; }
            set { m_sCommunityString = value; }
        }
        
    }
}
