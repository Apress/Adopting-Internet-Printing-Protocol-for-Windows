using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;
using System.Net;

namespace PrinterInfo
{
    public enum OID_TYPE_VALUES
    {
        INTEGER = 2,
        BIT_STRING = 3,
        OCTET_STRING = 4,
        NULL = 5,
        OBJECT_IDENTIFIER = 6,
        VARBIND = 48,
        IPADDRESS = 64,
        COUNTER32 = 65,
        GUAGE32 = 66,
        TIME_TICKS = 67,
        OPAQUE = 68,
        COUNTER64 = 70,
        NO_SUCH_OBJECT = 128,
        NO_SUCH_INSTANCE = 129,
        END_MIB_VIEW = 130
    };

    public class CSnmpResult :IComparable
    {
        private string m_sOid;

        public string OID
        {
            get { return m_sOid; }
            set { m_sOid = value; }
        }
        private string m_sValue;

        public string VALUE
        {
            get { return m_sValue; }
            set { m_sValue = value; }
        }
        private string m_sDescription;

        public string DESCRIPTION
        {
            get { return m_sDescription; }
            set { m_sDescription = value; }
        }

        private int m_iOidType;

        public int OID_TYPE
        {
            get { return m_iOidType; }
            set { m_iOidType = value; }
        }

        ///<remarks>
        /// Required interface for IComparable
        /// Implements CompareTo method for object CSnmpResult
        /// Required for sorting (in this case insensitive by oid)
        ///</remarks>
        public int CompareTo(object other)
        {
            return string.Compare(this.OID, ((CSnmpResult)other).OID, true);
        }

        public CSnmpResult()
        {
            OID = string.Empty;
            VALUE = string.Empty;
            DESCRIPTION = string.Empty;
            OID_TYPE = 0;
        }
    }
}
