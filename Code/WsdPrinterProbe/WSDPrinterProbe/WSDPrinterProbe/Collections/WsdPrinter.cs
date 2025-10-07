using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace WSDPrinterProbe
{
    public class WsdPrinter : IEnumerable<WsdProperty>
    {

        public const int npos = -1;
        private List<WsdProperty> wsdProperties = new List<WsdProperty>();
        private string m_sIpAddress;
        private string m_sUri;
        private string m_sGuid;

        public string IpAddress { get => m_sIpAddress; set => m_sIpAddress = value; }
        public string Uri { get => m_sUri; set => m_sUri = value; }
        public string Guid { get => m_sGuid; set => m_sGuid = value; }

        public WsdPrinter(string ipa, string guid) 
        {
            Uri = ipa;
            Guid = guid;
            int loc = ipa.IndexOf(":");
            if ((loc != npos))
            {
                IpAddress = ipa.Substring(0, loc);
            }
            else
            {
                IpAddress = ipa;
            }
        }

        /// <summary>
        /// AddWsdXmlProperty
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddWsdXmlProperty(string name, string value)
        {
            if (Find(name) == null)
            {
                WsdProperty prop = new WsdProperty(name, value);
                wsdProperties.Add(prop);
            }
        }

        public IEnumerator<WsdProperty> GetEnumerator()
        {
            return wsdProperties.GetEnumerator();
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
        public WsdProperty Find(string name)
        {
            return wsdProperties.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
