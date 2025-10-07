using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_GPA_CH3_WPF
{
    public class PrinterAttribute : IEnumerable<string>
    {
        private string m_name;
        private List<string> m_lstValues = new List<string>();
        public PrinterAttribute(string n, string v) 
        { 
            processNewAttribute(n, v);
        }

        private void processNewAttribute(string n, string v)
        {
            try
            {
                string[] values = v.Split(',');
                Name = n;
                foreach (string s in values)
                {
                    m_lstValues.Add(s);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not process new attribute, reason: {ex.Message}");
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

    }
}
