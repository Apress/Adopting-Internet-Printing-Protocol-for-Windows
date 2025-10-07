using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{
    public class PrinterAttributes : IEnumerable<PrinterAttribute>

    {
        List<PrinterAttribute> m_lstAttributes = new List<PrinterAttribute>();
        public PrinterAttributes() { }

        public List<PrinterAttribute> AttributesList => m_lstAttributes;

        public void AddAttribute(PrinterAttribute attr)
        {
            try
            {
                PrinterAttribute pa = Find(attr.Name);
                if (pa == null)
                {
                    AttributesList.Add(attr);
                }
                else
                {

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
        public PrinterAttribute Find(string name)
        {
            return m_lstAttributes.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsSupported(string name, string value)
        {
            PrinterAttribute pa = Find(name);
            if(pa == null) 
            { 
                return false; 
            }
            else
            {
                return pa.ContainsValue(value);
            }
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
