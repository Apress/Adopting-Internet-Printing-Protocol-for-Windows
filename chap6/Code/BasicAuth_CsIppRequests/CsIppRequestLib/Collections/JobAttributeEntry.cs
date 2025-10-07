using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class JobAttributeEntry
    {
        private string m_sName;
        private object m_oValue;
        private byte m_bTag;
        public JobAttributeEntry(string sName, object oValue, byte bTag)
        {
            Name = sName;
            Value = oValue;
            Tag = bTag;
        }

        public string Name { get => m_sName; set => m_sName = value; }
        public object Value { get => m_oValue; set => m_oValue = value; }
        public byte Tag { get => m_bTag; set => m_bTag = value; }

        public string ValueToString()
        {
            string sv = string.Format("{0}", Value);
            return sv;
        }
    }
}
