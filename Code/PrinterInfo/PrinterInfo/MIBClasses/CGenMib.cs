using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace PrinterInfo
{
    /// <summary>
    /// CGenMib
    /// 
    /// Approximation of the SNMP MIB-2 System
    /// </summary>
    public class CGenMib : CSnmp
    {
        private Hashtable ht_General = new Hashtable();
        private string m_sSystemDescription = string.Empty;
        private string m_sSysObjectID = string.Empty;

        const string MIB2 = "1.3.6.1.2.1.1"; //mib-2

        public CGenMib(int iMaxValues, string cs): base(MIB2, iMaxValues, cs)
        {

        }

        public void GetValues()
        {
            GetGeneralSettings();
        }

        public string SYSTEM_DESCRIPTION
        {
            get { return m_sSystemDescription; }
            set { m_sSystemDescription = value; }
        }

        public string SYSTEM_OBJECT_ID
        {
            get { return m_sSysObjectID; }
            set { m_sSysObjectID = value; }
        }

        public void PrintValues()
        {
            foreach (DictionaryEntry de in ht_General)
            {
                Console.WriteLine(de.Key.ToString() + ": " + de.Value.ToString());
            }
        }

        /// <summary>
        /// GetGeneralSettings
        /// </summary>
        /// <param name="_cso"></param>
        private void GetGeneralSettings()
        {
            ht_General.Add("max", "1");
            var x = Find(BASE_OID + ".1.0");
            if (x != null)
            {
                ht_General.Add("sysDescr", x.VALUE.ToString());
                SYSTEM_DESCRIPTION = x.VALUE.ToString();
            }
            x = Find(BASE_OID + ".2.0");
            if (x != null)
            {
                ht_General.Add("SysObjectID", x.VALUE);
                SYSTEM_OBJECT_ID = x.VALUE;
            }
            x = Find(BASE_OID + ".3.0");
            if (x != null)
                ht_General.Add("sysUpTime", x.VALUE.ToString());
            x = Find(BASE_OID + ".4.0");
            if (x != null)
                ht_General.Add("sysContact", x.VALUE);
            x = Find(BASE_OID + ".5.0");
            if (x != null)
                ht_General.Add("sysName", x.VALUE);
            x = Find(BASE_OID + ".6.0");
            if (x != null)
                ht_General.Add("sysLocation", x.VALUE);
            x = Find(BASE_OID + ".7.0");
            if (x != null)
                ht_General.Add("sysServices", x.VALUE);

        }
    }
}
