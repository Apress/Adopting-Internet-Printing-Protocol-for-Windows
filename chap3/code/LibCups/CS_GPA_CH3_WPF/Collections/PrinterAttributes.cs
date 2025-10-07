using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_GPA_CH3_WPF
{
    public class PrinterAttributes : IEnumerable<PrinterAttribute>

    {
        List<PrinterAttribute> m_lstAttributes = new List<PrinterAttribute>();
        public PrinterAttributes() { }

        public List<PrinterAttribute> AttributesList => m_lstAttributes;

        public void AddAttribute(string attString)
        {
            try
            {
                string[] attributes = attString.Split('|');
                PrinterAttribute pa = Find(attributes[0]);
                if (pa == null)
                {
                    PrinterAttribute npa = new PrinterAttribute(attributes[0], attributes[1]);
                    AttributesList.Add(npa);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not add attribute, reason: {ex.Message}");
            }
        }


        public IEnumerator<PrinterAttribute> GetEnumerator()
        {
            return m_lstAttributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private PrinterAttribute Find(string name)
        {
            return m_lstAttributes.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// GetAttributeValues
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> GetAttributeValues(string name)
        {
            PrinterAttribute pa = Find(name);
            if (pa == null)
                return null;
            else
            {
                return pa.AttributeValues;
            }
        }


    }
}
