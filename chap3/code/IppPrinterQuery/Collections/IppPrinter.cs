using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IppPrinterQuery
{
    public class IppPrinter : IEnumerable<IppAttributeObject>
    {
        private string m_printer_name;
        private short m_status_code;
        private string m_version;
        private List<string> m_requestedattributes = new List<string>();
        private List<IppAttributeObject> m_lstAttributeObjects = new List<IppAttributeObject>();

        public IppPrinter(string printer_name, List<string> _attributes) 
        {
            PrinterName = printer_name;
            m_requestedattributes = _attributes;
        }

        public string PrinterName { get => m_printer_name; set => m_printer_name = value; }
        public short StatusCode { get => m_status_code; set => m_status_code = value; }
        public string Version { get => m_version; set => m_version = value; }


        /// <summary>
        /// AddNewAttribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddNewAttribute(string name, string value, byte tag)
        {
            if(IsRequested(name)==true)
            {
                IppAttributeObject ao = Find(name);
                if (ao != null)
                {
                    //add to existing attributeobject
                    ao.AddValue(value);
                }
                else
                {
                    // this is a new attributeobject
                    IppAttributeObject aon = new IppAttributeObject(name, value, tag);
                    AttributeObjects.Add(aon);  
                }
            }
        }

        public List<IppAttributeObject> AttributeObjects => m_lstAttributeObjects;

        public IEnumerator<IppAttributeObject> GetEnumerator()
        {
            return m_lstAttributeObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// IsRequested
        /// Is the attribute in the requested attributes list?
        /// </summary>
        /// <param name="attribute_name"></param>
        /// <returns></returns>
        private bool IsRequested(string attribute_name)
        {
            return m_requestedattributes.Any(s => s.Contains(attribute_name));
        }

        private IppAttributeObject Find(string name)
        {
            return m_lstAttributeObjects.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
