using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class PrinterAttribute : IEnumerable<string>
    {
        private string m_name = string.Empty;
        private List<string> m_lstValues = new List<string>();
        public PrinterAttribute(string n, string v) 
        { 
            processNewAttribute(n, v);
        }

        private void processNewAttribute(string n, string v)
        {
            try
            {
                Name = n;
                m_lstValues.Add(v);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not process new attribute, reason: {ex.Message}");
            }
        }

        public void AddAttributeValue(string val)
        {
            if (ContainsValue(val) == false)
            {
                m_lstValues.Add(val);
            }
        }

        public string Name { get => m_name; set => m_name = value; }

        public List<string> AttributeValues => m_lstValues;


        public IEnumerator<string> GetEnumerator()
        {
            return m_lstValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsValue(string value)
        {
            foreach(string sv in AttributeValues)
            {
                if(string.Compare(sv, value, true)  == 0)
                {
                    return true;    
                }
            }
            return false;
        }

    }
}
