using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using SnmpSharpNet;
using System.Net;
using System.IO;

namespace PrinterInfo
{
    public class CSnmp
    {
        private List<CSnmpResult> m_List;
        private string m_sBaseOid = string.Empty;   
        private Oid m_rootOid, m_lastOid;
        protected AgentParameters m_param;
        protected UdpTarget m_target;
        protected string m_sHostAddress = string.Empty;
        protected string m_CommunityString = string.Empty;
        private int iMaxReturn = 100;
        public const int npos = -1;
        protected int iMax = 0;
        private string m_sVendorOid = string.Empty;
        private string m_sVendorDigit = string.Empty;

        public CSnmp(string sBase, int iMaxValues, string cs)
        {
            m_List = new List<CSnmpResult>();
            BASE_OID = sBase;
            MAX_RETURN = iMaxValues;
            COMMUNITY_STRING = cs;
        }

        public string VENDOR_DIGIT
        {
            get { return m_sVendorDigit; }
            set { m_sVendorDigit = value; }
        }

        public string VENDOR_OID
        {
            get { return m_sVendorOid; }
            set { m_sVendorOid = value; }
        }

        public string HOSTADDRESS
        {
            get { return m_sHostAddress; }
            set { m_sHostAddress = value; }
        }

        public string BASE_OID
        {
            get { return m_sBaseOid; }
            set { m_sBaseOid = value; }
        }

        public int MAX_RETURN
        {
            get { return iMaxReturn; }
            set { iMaxReturn = value; }
        }

        public string COMMUNITY_STRING
        {
            get { return m_CommunityString; }
            set { m_CommunityString = value; }
        }

        public int SNMP_MIB_RESULTS
        {
            get { return m_List.Count; }
        }

        public void GetInfo(string ipaddress)
        {
            HOSTADDRESS = ipaddress;
            IpAddress agent = new IpAddress(HOSTADDRESS);
            OctetString community = new OctetString(COMMUNITY_STRING);
            // Define agent parameters class
            m_param = new AgentParameters(community);
            m_rootOid = new Oid(m_sBaseOid);
            m_lastOid = (Oid)m_rootOid.Clone();
            m_target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            m_param.Version = SnmpVersion.Ver2;
            GetResults();
        }

        private void GetResults()
        {
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetBulk);
            // In this example, set NonRepeaters value to 0
            pdu.NonRepeaters = 0;
            // MaxRepetitions tells the agent how many Oid/Value pairs to return in the response.
            pdu.MaxRepetitions = MAX_RETURN;

            try
            {
                while (m_lastOid != null)
                {
                    // When Pdu class is first constructed, RequestId is set to 0
                    // and during encoding id will be set to the random value
                    // for subsequent requests, id will be set to a value that
                    // needs to be incremented to have unique request ids for each
                    // packet
                    if (pdu.RequestId != 0)
                    {
                        pdu.RequestId += 1;
                    }
                    // Clear Oids from the Pdu class.
                    pdu.VbList.Clear();
                    // Initialize request PDU with the last retrieved Oid
                    pdu.VbList.Add(m_lastOid);
                    // Make SNMP request
                    SnmpV2Packet result = (SnmpV2Packet)m_target.Request(pdu, m_param);
                    // You should catch exceptions in the Request if using in real application.

                    // If result is null then agent didn't reply or we couldn't parse the reply.
                    if (result != null)
                    {
                        // ErrorStatus other then 0 is an error returned by 
                        // the Agent - see SnmpConstants for error definitions
                        if (result.Pdu.ErrorStatus != 0)
                        {
                            // agent reported an error with the request
                            throw new Exception("Error in SNMP reply, error status: " + result.Pdu.ErrorStatus + " Error index: " + result.Pdu.ErrorIndex);
                        }
                        else
                        {
                            // Walk through returned variable bindings
                            foreach (Vb v in result.Pdu.VbList)
                            {
                                // Check that retrieved Oid is "child" of the root OID
                                if (m_rootOid.IsRootOf(v.Oid))
                                {
                                    CSnmpResult res = new CSnmpResult();
                                    res.OID = v.Oid.ToString();
                                    res.VALUE = v.Value.ToString();
                                    res.OID_TYPE = v.Value.Type;
                                    m_List.Add(res);
                                    if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                        m_lastOid = null;
                                    else
                                        m_lastOid = v.Oid;
                                }
                                else
                                {
                                    // we have reached the end of the requested
                                    // MIB tree. Set lastOid to null and exit loop
                                    m_lastOid = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("No response received from SNMP agent.");
                    }
                }
            }
            catch (Exception exsnmp)
            {
                throw new Exception("Error on snmp attempt: " + exsnmp.Message);
            }
            finally
            {
                m_lastOid = null;
                m_target.Close();
            }
        }

        /// <summary>
        /// Find
        /// Look for a CSnmpResult by OID
        /// This is case insensitive
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public CSnmpResult Find(string oid)
        {
            return m_List.FirstOrDefault(x => String.Equals(x.OID, oid, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// EnumGroup
        /// Enumerate through group of known entries and add them to the given
        /// Hashtable.
        /// </summary>
        /// <param name="oid_base"></param>
        /// <param name="max_index"></param>
        /// <param name="ht"></param>
        /// <param name="s_HashEntry"></param>
        protected void EnumGroup(string oid_base, int max_index, Hashtable ht, string s_HashEntry)
        {
            string new_oid = string.Empty;
            int pos = 0;

            for (int i = 1; i <= max_index; i++)
            {
                pos = oid_base.LastIndexOf('.');
                if (pos == npos)
                    break;
                new_oid = oid_base.Substring(0, pos);
                string new_index = "." + i.ToString();
                new_oid += new_index;
                var x = Find(new_oid);
                if (x == null)
                    break;
                else
                {
                    ht.Add(s_HashEntry + i.ToString(), x.VALUE);
                }
            }
        }


        /// <summary>
        ///  GetMaxValueOfGroup
        ///  This method returns the total number of items in an oid branch group (index start at 1).
        /// </summary>
        /// <param name="_cso"></param>
        /// <param name="oid_base"></param>
        /// <returns></returns>
        protected int GetMaxValueOfGroup(string oid_base)
        {
            int pos = oid_base.LastIndexOf('.');
            string new_oid = string.Empty;
            int num = 1;
            if (pos == npos)
                return 0;

            for (; ; )
            {
                new_oid = oid_base.Substring(0, pos);
                string new_index = "." + num.ToString();
                new_oid += new_index;
                var x = Find(new_oid);
                if (x == null)
                    break;
                else
                    num++;
            }
            return num - 1;
        }
      

        static string GetOidType(int v)
        {
            switch (v)
            {
                case 2:
                    return "INTEGER";
                case 3:
                    return "BIT_STRING";
                case 4:
                    return "OCTET_STRING";
                case 5:
                    return "NULL";
                case 6:
                    return "OBJECT_IDENTIFIER";
                case 48:
                    return "VARBIND";
                case 64:
                    return "IPADDRESS";
                case 65:
                    return "COUNTER32";
                case 66:
                    return "GUAGE32";
                case 67:
                    return "TIME_TICKS";
                case 68:
                    return "OPAQUE";
                case 70:
                    return "COUNTER64";
                case 128:
                    return "NO_SUCH_OBJECT";
                case 129:
                    return "NO_SUCH_INSTANCE";
                case 130:
                    return " END_MIB_VIEW";
                default:
                    return "UNKNOWN";
            }
        }
    }
}
