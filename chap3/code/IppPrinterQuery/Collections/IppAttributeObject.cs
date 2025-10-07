using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IppPrinterQuery
{
    public class IppAttributeObject : IEnumerable<string>
    {
        private string m_name;
        private byte m_valuetag;
        private int m_count;
        private List<string> m_lstValues = new List<string>();    

        public IppAttributeObject(string n)
        {
            AddValue(n);
        }

        public IppAttributeObject(string n, string value, byte vtag)
        {
            m_name = n;
            ValueTag = vtag;
            AddValue(value);    
        }

        public IEnumerator<string> GetEnumerator()
        {
            return m_lstValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Name { get => m_name; set => m_name = value; }
        public byte ValueTag { get =>  m_valuetag; set => m_valuetag = value; }

        public int Count 
        {
            get => m_lstValues.Count;
        }

        public void AddValue(string value)
        {
            m_lstValues.Add(value);   
        }
    }
}
